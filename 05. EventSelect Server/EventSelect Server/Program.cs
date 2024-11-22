using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EventSelect_Server
{
    internal class Program
    {
        static List<Socket> clients = new();
        static readonly object CS = new();

        static void Main(string[] args)
        {
            Console.WriteLine("S to start Server \nQ to Exit \nCheck to view clients many");

            while (true)
            {
                var input = Console.ReadLine() ?? String.Empty;

                if (input.ToLower() == "s")
                {
                    _ = Task.Run(() => StartServer());
                }
                else if (input.ToLower() == "q")
                {
                    Console.WriteLine("Shutting down server...");
                    lock (CS)
                    {
                        foreach (var client in clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        clients.Clear();
                    }
                    break;
                }
                else if (input.ToLower() == "check")
                {
                    lock (CS)
                    {
                        Console.WriteLine($"Connected clients: {clients.Count - 1}");
                    }
                }
            }
        }

        static async Task StartServer()
        {
            Socket listening = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listening.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listening.Listen();

            Console.WriteLine("Server is ready to connect with clients.");
            Console.WriteLine($"Server Address: {listening.LocalEndPoint}");

            lock (CS)
            {
                clients.Add(listening);
            }

            while (true)
            {
                List<Socket> readList;

                lock (CS)
                {
                    readList = new List<Socket>(clients);
                }

                Socket.Select(readList, null, null, 1000);

                foreach (var socket in readList)
                {
                    if (socket == listening)
                    {
                        // Accept new client connection 클라이언트에선 리스닝 서버에서 Accept가 이뤄지기 전에 Accept가 된 것처럼 간주됨.
                        // 이것으로 미루어 보아 tcp 연결과정에서 연결완료를 기다리지 않는다는 것을 알 수 있음 
                        // 하지만 서버에서의 Accept가 이루어지지 않으면 실제로 연결이 성립되진 않음
                        var client = listening.Accept();
                        Console.WriteLine($"New client connected: {client.RemoteEndPoint}");

                        lock (CS)
                        {
                            clients.Add(client);
                            Console.WriteLine($"Connected clients: {clients.Count - 1}");
                        }

                    }
                    else
                    {
                        _ = Task.Run(() => HandleClient(socket));
                    }
                }
            }
        }

        static async Task HandleClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            try
            {
                int bytesRead = client.Receive(buffer);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Message from {client.RemoteEndPoint}: {message}");
                    Broadcast(message, client);
                }
                else if (bytesRead == 0)
                {
                    DisconnectClient(client);
                }
            }
            catch (Exception ex)
            {
                DisconnectClient(client);
            }
        }

        static void Broadcast(string message, Socket excludeClient)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (CS)
            {
                foreach (var client in clients.ToList())
                {
                    if (client != excludeClient && client != clients[0])
                    {
                        try
                        {
                            if (client.Connected)
                            {
                                client.Send(buffer);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}: {ex.Message}");
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
                    return; // 이미 처리된 클라이언트라면 반환
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
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Socket already disposed.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while disconnecting client: {ex.Message}");
                }
            }
        }

    }
}
