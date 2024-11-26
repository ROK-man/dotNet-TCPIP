using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileServer
{
    public class Message
    {
        public int Protocol { get; set; }  // 1=메시지, 2=파일 리스트 요청, 3=파일 요청
        public int FileIndex { get; set; }   // 파일 인덱스 (파일 요청 시 사용)
        public string Payload { get; set; }  // 메시지 내용 (메시지 전송 시 사용)

        public static byte[] Serialize(Message obj)
        {
            var jsonData = JsonSerializer.SerializeToUtf8Bytes(obj);
            var lengthBytes = BitConverter.GetBytes(jsonData.Length);
            return lengthBytes.Concat(jsonData).ToArray(); // 헤더(4바이트) + JSON 데이터
        }

        public static Message Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<Message>(data);
        }
    }

    internal class Program
    {
        private static readonly List<Socket> Clients = new();
        private static readonly string FilesDirectory = "SharedFiles";

        static async Task Main(string[] args)
        {
            if (!Directory.Exists(FilesDirectory))
            {
                Directory.CreateDirectory(FilesDirectory);
                Console.WriteLine($"Directory {FilesDirectory} created for file sharing.");
            }

            Console.WriteLine("Starting server...");
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listener.Listen(100);

            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                var clientSocket = await listener.AcceptAsync();
                Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");
                lock (Clients) Clients.Add(clientSocket);

                _ = Task.Run(() => HandleClient(clientSocket));
            }
        }

        private static async Task HandleClient(Socket clientSocket)
        {
            var buffer = new List<byte>();
            try
            {
                while (true)
                {
                    var tempBuffer = new byte[4096];
                    int receivedBytes = await clientSocket.ReceiveAsync(tempBuffer, SocketFlags.None);
                    if (receivedBytes == 0) break;

                    buffer.AddRange(tempBuffer.Take(receivedBytes));

                    while (buffer.Count >= 4) // 최소한 헤더 크기(4바이트)가 있어야 함
                    {
                        int messageLength = BitConverter.ToInt32(buffer.Take(4).ToArray(), 0);

                        if (buffer.Count < 4 + messageLength) break; // 메시지가 아직 다 도착하지 않음

                        var messageData = buffer.Skip(4).Take(messageLength).ToArray();
                        buffer.RemoveRange(0, 4 + messageLength);

                        var message = Message.Deserialize(messageData);
                        await ProcessMessage(clientSocket, message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {clientSocket.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                DisconnectClient(clientSocket);
            }
        }

        private static async Task ProcessMessage(Socket clientSocket, Message message)
        {
            switch (message.Protocol)
            {
                case 1: // 메시지 브로드캐스트
                    Console.WriteLine($"Message from {clientSocket.RemoteEndPoint}: {message.Payload}");
                    BroadcastMessage(message.Payload, clientSocket);
                    break;

                case 2: // 파일 리스트 요청
                    await SendFileList(clientSocket);
                    break;

                case 3: // 파일 요청
                    await SendFile(clientSocket, message.FileIndex);
                    break;

                default:
                    Console.WriteLine("Unknown command received.");
                    break;
            }
        }

        private static void BroadcastMessage(string message, Socket excludeClient)
        {
            var broadcastMessage = new Message { Protocol = 1, Payload = message };
            byte[] buffer = Message.Serialize(broadcastMessage);

            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    if (client != excludeClient)
                    {
                        try
                        {
                            client.Send(buffer);
                        }
                        catch
                        {
                            DisconnectClient(client);
                        }
                    }
                }
            }
        }

        private static async Task SendFileList(Socket clientSocket)
        {
            var files = Directory.GetFiles(FilesDirectory);
            var fileList = string.Join(Environment.NewLine, files.Select((f, i) => $"{i}: {Path.GetFileName(f)}"));

            var response = new Message { Protocol = 2, Payload = fileList };
            await clientSocket.SendAsync(Message.Serialize(response), SocketFlags.None);
        }

        private static async Task SendFile(Socket clientSocket, int fileIndex)
        {
            var files = Directory.GetFiles(FilesDirectory);

            if (fileIndex < 0 || fileIndex >= files.Length)
            {
                var error = new Message { Protocol = 1, Payload = "Invalid file index." };
                await clientSocket.SendAsync(Message.Serialize(error), SocketFlags.None);
                return;
            }

            string filePath = files[fileIndex];
            string fileName = Path.GetFileName(filePath);
            byte[] fileData = File.ReadAllBytes(filePath);

            var startMessage = new Message { Protocol = 3, Payload = fileName };
            await clientSocket.SendAsync(Message.Serialize(startMessage), SocketFlags.None);

            await clientSocket.SendAsync(fileData, SocketFlags.None);
            Console.WriteLine($"File {fileName} sent successfully.");
        }

        private static void DisconnectClient(Socket client)
        {
            lock (Clients) Clients.Remove(client);
            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch { }
        }
    }
}
