using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client
{
    public class ProtocolMessage
    {
        public int CommandType { get; set; }  // 1=메시지, 2=파일 리스트 요청, 3=파일 요청
        public int FileIndex { get; set; }   // 파일 인덱스 (파일 요청 시 사용)
        public string Message { get; set; }  // 메시지 내용 (메시지 전송 시 사용)

        public static byte[] Serialize(ProtocolMessage obj)
        {
            var jsonData = JsonSerializer.SerializeToUtf8Bytes(obj);
            var lengthBytes = BitConverter.GetBytes(jsonData.Length);
            return lengthBytes.Concat(jsonData).ToArray(); // 헤더(4바이트) + JSON 데이터
        }

        public static ProtocolMessage Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<ProtocolMessage>(data);
        }
    }

    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 25000));
                Console.WriteLine("Connected to server!");

                _ = Task.Run(() => ReceiveMessages(clientSocket)); // 비동기 수신 작업 실행

                while (true)
                {
                    Console.WriteLine("Enter command (1=Message, 2=List Files, 3=Download File, Exit to Quit):");
                    var input = Console.ReadLine();

                    if (input?.ToLower() == "exit")
                        break;

                    var message = new ProtocolMessage();

                    switch (input)
                    {
                        case "1": // 메시지 보내기
                            Console.Write("Enter message: ");
                            message.CommandType = 1;
                            message.Message = Console.ReadLine();
                            break;

                        case "2": // 파일 목록 요청
                            message.CommandType = 2;
                            break;

                        case "3": // 파일 다운로드 요청
                            Console.Write("Enter file index: ");
                            message.CommandType = 3;
                            if (int.TryParse(Console.ReadLine(), out int fileIndex))
                            {
                                message.FileIndex = fileIndex;
                            }
                            else
                            {
                                Console.WriteLine("Invalid file index.");
                                continue;
                            }
                            break;

                        default:
                            Console.WriteLine("Invalid command.");
                            continue;
                    }

                    byte[] buffer = ProtocolMessage.Serialize(message);
                    await clientSocket.SendAsync(buffer, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                clientSocket.Close();
            }
        }

        private static async Task ReceiveMessages(Socket clientSocket)
        {
            var buffer = new List<byte>();
            try
            {
                while (true)
                {
                    var tempBuffer = new byte[4096];
                    int receivedBytes = await clientSocket.ReceiveAsync(tempBuffer, SocketFlags.None);

                    if (receivedBytes == 0) break; // 연결 종료
                    buffer.AddRange(tempBuffer.Take(receivedBytes));

                    while (buffer.Count >= 4) // 최소한 헤더 크기(4바이트)가 있어야 함
                    {
                        int messageLength = BitConverter.ToInt32(buffer.Take(4).ToArray(), 0);

                        if (buffer.Count < 4 + messageLength) break; // 메시지가 아직 다 도착하지 않음

                        var messageData = buffer.Skip(4).Take(messageLength).ToArray();
                        buffer.RemoveRange(0, 4 + messageLength);

                        var message = ProtocolMessage.Deserialize(messageData);

                        await ProcessMessage(clientSocket, message);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Disconnected from server.");
            }
        }

        private static async Task ProcessMessage(Socket clientSocket, ProtocolMessage message)
        {
            switch (message.CommandType)
            {
                case 1: // 일반 메시지
                    Console.WriteLine($"Server Message: {message.Message}");
                    break;

                case 2: // 파일 리스트
                    Console.WriteLine("File List:");
                    Console.WriteLine(message.Message);
                    break;

                case 3: // 파일 데이터
                    Console.WriteLine($"Receiving file: {message.Message}");
                    await SaveFile(clientSocket, message.Message);
                    break;

                default:
                    Console.WriteLine("Unknown command from server.");
                    break;
            }
        }

        private static async Task SaveFile(Socket clientSocket, string fileName)
        {
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int receivedBytes;

                while ((receivedBytes = await clientSocket.ReceiveAsync(buffer, SocketFlags.None)) > 0)
                {
                    fs.Write(buffer, 0, receivedBytes);

                    // 데이터가 모두 전송된 신호로 데이터 크기가 버퍼보다 작으면 완료
                    if (receivedBytes < buffer.Length)
                        break;
                }
            }

            Console.WriteLine($"File {fileName} saved to {savePath}");
        }
    }
}
