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
    public class ProtocolMessage
    {
        public int CommandType { get; set; }  // 1=메시지, 2=파일 리스트 요청, 3=파일 요청
        public int FileIndex { get; set; }   // 파일 인덱스 (파일 요청 시 사용)
        public string Message { get; set; }  // 메시지 내용 (메시지 전송 시 사용)

        public static byte[] Serialize(ProtocolMessage obj)
        {
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public static ProtocolMessage Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<ProtocolMessage>(data);
        }
    }

    internal class Program
    {
        private static readonly List<Socket> Clients = new();
        private static readonly string FilesDirectory = "SharedFiles";

        static void Main(string[] args)
        {
            if (!Directory.Exists(FilesDirectory))
            {
                Directory.CreateDirectory(FilesDirectory);
                Console.WriteLine($"Directory {FilesDirectory} created for file sharing.");
            }

            Console.WriteLine("Starting server...");
            StartServer();
        }

        private static void StartServer()
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listener.Listen(100);

            Console.WriteLine("Server started. Waiting for clients...");

            while (true)
            {
                var clientSocket = listener.Accept();
                Console.WriteLine($"Client connected: {clientSocket.RemoteEndPoint}");
                lock (Clients) Clients.Add(clientSocket);

                _ = Task.Run(() => HandleClient(clientSocket));
            }
        }

        private static void HandleClient(Socket clientSocket)
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[4096];
                    int receivedBytes = clientSocket.Receive(buffer);

                    if (receivedBytes > 0)
                    {
                        var message = ProtocolMessage.Deserialize(buffer.Take(receivedBytes).ToArray());

                        switch (message.CommandType)
                        {
                            case 1:
                                Console.WriteLine($"Message from {clientSocket.RemoteEndPoint}: {message.Message}");
                                BroadcastMessage(message.Message, clientSocket);
                                break;

                            case 2:
                                SendFileList(clientSocket);
                                break;

                            case 3:
                                SendFile(clientSocket, message.FileIndex);
                                break;

                            default:
                                Console.WriteLine("Unknown command received.");
                                break;
                        }
                    }
                    else
                    {
                        break;
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

        private static void BroadcastMessage(string message, Socket excludeClient)
        {
            var broadcastMessage = new ProtocolMessage { CommandType = 1, Message = message };
            byte[] buffer = ProtocolMessage.Serialize(broadcastMessage);

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

        private static void SendFileList(Socket clientSocket)
        {
            var files = Directory.GetFiles(FilesDirectory);
            var fileList = string.Join(Environment.NewLine, files.Select((f, i) => $"{i}: {Path.GetFileName(f)}"));

            var response = new ProtocolMessage { CommandType = 2, Message = fileList };
            clientSocket.Send(ProtocolMessage.Serialize(response));
        }

        private static void SendFile(Socket clientSocket, int fileIndex)
        {
            var files = Directory.GetFiles(FilesDirectory);

            if (fileIndex < 0 || fileIndex >= files.Length)
            {
                var error = new ProtocolMessage { CommandType = 1, Message = "Invalid file index." };
                clientSocket.Send(ProtocolMessage.Serialize(error));
                return;
            }

            string filePath = files[fileIndex];
            string fileName = Path.GetFileName(filePath);
            byte[] fileData = File.ReadAllBytes(filePath);

            try
            {
                var startMessage = new ProtocolMessage { CommandType = 3, Message = fileName };
                clientSocket.Send(ProtocolMessage.Serialize(startMessage));

                clientSocket.Send(fileData);
                Console.WriteLine($"File {fileName} sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while sending file: {ex.Message}");
            }
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
