using System;
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
            return JsonSerializer.SerializeToUtf8Bytes(obj);
        }

        public static ProtocolMessage Deserialize(byte[] data)
        {
            return JsonSerializer.Deserialize<ProtocolMessage>(data);
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, 25000));
                Console.WriteLine("Connected to server!");

                Task.Run(() => ReceiveMessages(clientSocket));

                while (true)
                {
                    Console.WriteLine("Enter command (1=Message, 2=List Files, 3=Download File, Exit to Quit):");
                    var input = Console.ReadLine();

                    if (input?.ToLower() == "exit")
                    {
                        break;
                    }

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
                    clientSocket.Send(buffer);
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

        private static void ReceiveMessages(Socket clientSocket)
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
                            case 1: // 일반 메시지
                                Console.WriteLine($"Server Message: {message.Message}");
                                break;

                            case 2: // 파일 리스트
                                Console.WriteLine("File List:");
                                Console.WriteLine(message.Message);
                                break;

                            case 3: // 파일 데이터
                                Console.WriteLine("Receiving file...");
                                SaveFile(clientSocket, message.Message);
                                break;

                            default:
                                Console.WriteLine("Unknown command from server.");
                                break;
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Disconnected from server.");
            }
        }

        private static void SaveFile(Socket clientSocket, string fileName)
        {
            string savePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                byte[] buffer = new byte[4096];
                int receivedBytes;

                while ((receivedBytes = clientSocket.Receive(buffer)) > 0)
                {
                    fs.Write(buffer, 0, receivedBytes);

                    // 파일 전송 완료 신호 확인
                    if (receivedBytes < buffer.Length)
                        break;
                }
            }

            Console.WriteLine($"File {fileName} saved to {savePath}");
        }
    }
}
