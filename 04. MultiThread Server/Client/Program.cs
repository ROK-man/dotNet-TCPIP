using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                // 서버에 연결
                clientSocket.Connect(serverEndPoint);
                Console.WriteLine("Connected to server. Type 'exit' to disconnect.");

                // 서버로 메시지 보내는 스레드
                Thread sendThread = new Thread(() => SendMessages(clientSocket));
                sendThread.Start();

                // 서버로부터 메시지 수신하는 스레드
                Thread receiveThread = new Thread(() => ReceiveMessages(clientSocket));
                receiveThread.Start();

                // 메인 스레드는 보내기 및 수신 스레드가 종료될 때까지 대기
                sendThread.Join();
                receiveThread.Join();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
            }
            finally
            {
                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
                Console.WriteLine("Disconnected from server.");
            }
        }

        // 서버로 메시지를 보내는 메서드
        static void SendMessages(Socket clientSocket)
        {
            while (true)
            {
                string message = Console.ReadLine();

                // "exit" 명령어로 종료
                if (message.Trim().ToLower() == "exit")
                {
                    byte[] exitMessage = Encoding.UTF8.GetBytes(message);
                    clientSocket.Send(exitMessage, SocketFlags.None);
                    break;
                }

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(messageBytes, SocketFlags.None);
            }
        }

        // 서버로부터 메시지를 수신하는 메서드
        static void ReceiveMessages(Socket clientSocket)
        {
            var buffer = new byte[1024];
            int received;

            try
            {
                while ((received = clientSocket.Receive(buffer, SocketFlags.None)) > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Server: {message}");
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
            }
        }
    }
}
