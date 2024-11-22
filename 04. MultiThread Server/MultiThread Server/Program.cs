using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiThread_Server
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
                    lock(CS)
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
                        Console.WriteLine($"Connected clients: {clients.Count}");
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
            Console.WriteLine("Server Address: " + listening.LocalEndPoint);

            while (true)
            {
                var handler = await listening.AcceptAsync();
                Console.WriteLine($"New client connected: {handler.RemoteEndPoint}");

                lock (CS)
                {
                    clients.Add(handler);
                    Console.WriteLine($"Connected clients: {clients.Count}");
                }

                _ = Task.Run(() => HandleClient(handler));
            }
        }

        static async Task HandleClient(Socket client)
        {
            try
            {
                await Receive(client);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with client {client.RemoteEndPoint}: {ex.Message}");
            }
            finally
            {
                lock (CS)
                {
                    clients.Remove(client);
                }
                Console.WriteLine($"{client.RemoteEndPoint} disconnected.");
                client.Close();
            }
        }

        static async Task Receive(Socket client)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead;

                    bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (bytesRead > 0)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"{client.RemoteEndPoint}: {message}");

                        _ = Task.Run(() => Send(message));
                    }
                    if(bytesRead == 0)
                    {
                        break;
                    }
                }
            }
            catch
            {
                return;
            }
        }

        static void Send(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (CS)
            {
                foreach (var client in clients.ToList())
                {
                    try
                    {
                        client.SendAsync(buffer, SocketFlags.None);
                    }
                    catch
                    {
                        Console.WriteLine($"Failed to send message to {client.RemoteEndPoint}");
                    }
                }
            }
        }


    }
}
