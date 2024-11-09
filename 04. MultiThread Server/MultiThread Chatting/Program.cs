using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace MultiThread_Chatting
{
    internal class Program
    {
        // 모든 클라이언트 소켓을 저장하는 리스트
        static List<Socket> clientSockets = new();
        static readonly object lockObject = new(); // 스레드 동기화용 객체

        // 메시지를 모든 클라이언트에게 브로드캐스트하는 함수
        static void BroadCast(string message, Socket excludeSocket)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            lock (lockObject)
            {
                foreach (var clientSocket in clientSockets)
                {
                    // 제외할 소켓은 메시지를 보내지 않음
                    if (clientSocket != excludeSocket)
                    {
                        try
                        {
                            clientSocket.Send(messageBytes, SocketFlags.None);
                        }
                        catch (SocketException)
                        {
                            // 소켓 전송 중 예외가 발생할 경우 처리
                            Console.WriteLine("Failed to send message to a client.");
                        }
                    }
                }
            }
        }

        // 클라이언트와의 통신을 처리하는 함수
        static void Communication(object? clientSocket)
        {
            if (clientSocket == null) return;

            var handler = (Socket)clientSocket;
            var buffer = new byte[1024];
            int received;

            try
            {
                while ((received = handler.Receive(buffer, SocketFlags.None)) > 0)
                {
                    var response = Encoding.UTF8.GetString(buffer, 0, received);
                    Console.WriteLine($"Socket server received message from {handler.RemoteEndPoint}: \"{response}\"");

                    // 모든 클라이언트에게 브로드캐스트
                    BroadCast($"{handler.RemoteEndPoint}: {response}", handler);

                    // "exit" 메시지 처리
                    if (response.Trim().ToLower() == "exit")
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"{handler.RemoteEndPoint} Client has sent closing message.");
                        Console.ResetColor();
                        break;
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Socket error: {ex.Message}");
            }
            finally
            {
                // 클라이언트가 연결을 종료하면 리스트에서 제거하고 소켓 닫기
                lock (lockObject)
                {
                    clientSockets.Remove(handler);
                }
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                PrintYellow($"Client disconnected.");
            }
        }

        static void Main(string[] args)
        {
            // 리스닝 소켓 생성 후 바인딩, 리슨
            Socket socket = new(SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000));
            socket.Listen();

            PrintYellow("Listening socket is waiting for a connection...");

            // 상시적으로 소켓 통신 연결 받아들인 후 리스트에 통신소켓 추가 후 스레드로 넘기기
            while (true)
            {
                var handler = socket.Accept();
                PrintYellow("Client connected: " + handler.RemoteEndPoint);

                // 클라이언트 소켓을 리스트에 추가
                lock (lockObject)
                {
                    clientSockets.Add(handler);
                }

                // 클라이언트와의 통신을 위한 스레드 시작
                Thread newConnect = new Thread(new ParameterizedThreadStart(Communication));
                newConnect.Start(handler);
            }
        }
        static void PrintYellow(String s)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(s);
            Console.ResetColor();
        }
    }
}
