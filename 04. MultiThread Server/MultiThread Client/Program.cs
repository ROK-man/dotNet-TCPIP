using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiThread_Client
{
    internal class Program
    {
        static Queue<String> messages = new();
        static readonly object cs = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("S to connect server \nQ to exit");

            while (true)
            {
                var input = Console.ReadLine();
                if (input.ToLower() == "s")
                {
                    await Communication();
                    Console.WriteLine("Disconnected with Server");
                }
                else if (input.ToLower() == "q")
                {
                    break;
                }
            }
        }

        static async Task Communication()
        {
            try
            {
                Socket socket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000));

                Console.WriteLine("Server Connected");

                if (socket.Connected)
                {
                    Receive(socket);
                    var s = await Send(socket);
                    if (s.Equals("exit"))
                    {
                        Console.WriteLine("Disconnecting...");

                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Server disconnected");
            }
        }

        static async Task<string> Send(Socket socket)
        {
            Console.WriteLine("If you want to disconnect, enter EXIT");
            while (true)
            {
                var message = Console.ReadLine();
                if (message.ToLower() == "exit")
                {
                    return new string("exit");
                }
                var buffer = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(buffer);
            }
        }

        static async Task Receive(Socket socket)
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                var byteToRead = await socket.ReceiveAsync(buffer);

                if (byteToRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, byteToRead);
                    lock (cs)
                    {
                        messages.Enqueue(message);
                        _ = Task.Run(() => Print());
                    }
                }
                if(byteToRead == 0)
                {
                    break;
                }
            }
        }
        

        static void Print()
        {
            lock (cs)
            {
                if (messages.Count > 0)
                {
                    Console.WriteLine("Server: " + messages.Dequeue());
                }
            }
        }
    }
}
