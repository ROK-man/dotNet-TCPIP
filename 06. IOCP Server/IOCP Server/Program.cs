using System.Net;
using System.Net.Sockets;
using System.Text;

namespace IOCP_Server
{
    internal class Program
    {
        static List<Socket> clients = new();
        static Socket listeningSocket;
        static readonly object CS = new();

        static void Main(string[] args)
        {
            Console.WriteLine("S to start Server \nQ to Exit \nCheck to view clients count");

            while (true)
            {
                var input = Console.ReadLine() ?? string.Empty;

                if (input.ToLower() == "s")
                {
                    _ = Task.Run(() => StartServer());
                }
                else if (input.ToLower() == "q")
                {
                    Console.WriteLine("Shutting down server...");
                    ShutdownServer();
                    break;
                }
                else if (input.ToLower() == "check")
                {
                    lock (CS)
                    {
                        Console.WriteLine($"Connected clients: {clients.Count}");
                    }
                }
            }
        }

        static void StartServer()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listeningSocket.Listen(100);

            Console.WriteLine("Server started. Waiting for clients...");
            Console.WriteLine($"Server Address: {listeningSocket.LocalEndPoint}");

            StartAccept();
        }

        static void StartAccept()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptCompleted;

            if (!listeningSocket.AcceptAsync(acceptEventArg))
            {
                // 비동기 작업이 즉시 완료된 경우 직접 호출
                AcceptCompleted(listeningSocket, acceptEventArg);
            }
        }

        static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket clientSocket = e.AcceptSocket;
                Console.WriteLine($"New client connected: {clientSocket.RemoteEndPoint}");

                lock (CS)
                {
                    clients.Add(clientSocket);
                }

                // 클라이언트 처리 시작
                StartReceive(clientSocket);

                // 다음 클라이언트 연결 대기
                StartAccept();
            }
            else
            {
                Console.WriteLine($"Accept failed: {e.SocketError}");
            }
        }

        static void StartReceive(Socket clientSocket)
        {
            var receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.SetBuffer(new byte[1024], 0, 1024);
            receiveEventArg.UserToken = clientSocket;
            receiveEventArg.Completed += IOCompleted;

            if (!clientSocket.ReceiveAsync(receiveEventArg))
            {
                // 비동기 작업이 즉시 완료된 경우 직접 호출
                IOCompleted(clientSocket, receiveEventArg);
            }
        }

        static void IOCompleted(object sender, SocketAsyncEventArgs e)
        {
            Socket clientSocket = (Socket)e.UserToken;

            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred > 0)
                {
                    // 데이터 수신
                    string message = Encoding.UTF8.GetString(e.Buffer, e.Offset, e.BytesTransferred);
                    Console.WriteLine($"Message from {clientSocket.RemoteEndPoint}: {message}");

                    // 메시지 브로드캐스트
                    Broadcast(message, clientSocket);

                    // 다음 데이터 수신 대기
                    StartReceive(clientSocket);
                }
                else
                {
                    // 클라이언트 연결 종료
                    DisconnectClient(clientSocket);
                }
            }
            else
            {
                Console.WriteLine($"Receive error: {e.SocketError}");
                DisconnectClient(clientSocket);
            }
        }

        static void Broadcast(string message, Socket excludeClient)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (CS)
            {
                foreach (var client in clients.ToList())
                {
                    if (client != excludeClient)
                    {
                        try
                        {
                            client.Send(buffer);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Broadcast failed to {client.RemoteEndPoint}: {ex.Message}");
                            DisconnectClient(client);
                        }
                    }
                }
            }
        }

        static void DisconnectClient(Socket client)
        {
            lock (CS)
            {
                if (!clients.Contains(client))
                {
                    return;
                }

                try
                {
                    Console.WriteLine($"Client {client.RemoteEndPoint} disconnected.");
                    clients.Remove(client);

                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                    }

                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while disconnecting client: {ex.Message}");
                }
            }
        }

        static void ShutdownServer()
        {
            lock (CS)
            {
                foreach (var client in clients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            client.Shutdown(SocketShutdown.Both);
                        }
                        client.Close();
                    }
                    catch
                    {
                        // 클라이언트 종료 중 발생하는 예외는 무시
                    }
                }
                clients.Clear();
            }

            listeningSocket?.Close();
            Console.WriteLine("Server shutdown completed.");
        }
    }
}
