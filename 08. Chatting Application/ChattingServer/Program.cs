using System.Net;
using System.Net.Sockets;

namespace ChattingServer
{
    internal class Program
    {
        static object lcs = new();
        static object bcs = new();
        static Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<Socket> sockets = new List<Socket>();
        static List<byte> buffer = new List<byte>();
        static void Main(string[] args)
        {
            Console.WriteLine("Enter \"START\" for running server");
            Console.WriteLine("Enter \"STOP\" for terminate server");
            Console.WriteLine("Enter \"CHECK\" for see how many people connected");
            Console.WriteLine("Enter \"QUIT\" for terminate program");

            string input;
            while (true)
            {
                input = Console.ReadLine() ?? "";
                if (input.ToLower().Equals("start"))
                {
                    _ = Task.Run(() => AcceptConnect());
                }
                else if (input.ToLower().Equals("stop"))
                {
                    Console.WriteLine("Are you sure to shutdown server?  ['p']");
                    input = Console.ReadLine() ?? "";
                    if (input.Equals("q"))
                    {
                        Console.WriteLine("OK, bye");
                        return;
                    }
                }
                else if (input.ToLower().Equals("check"))
                {

                }
                else if (input.ToLower().Equals("quit"))
                {
                    Console.WriteLine("Are you sure to quit?  ['q']");
                    input = Console.ReadLine() ?? "";
                    if (input.Equals("q"))
                    {
                        Console.WriteLine("OK, bye");
                        return;
                    }
                }
                else
                {
                    continue;
                }
            }
        }

        static void AcceptConnect()
        {
            try
            {
                serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
                serverSocket.Listen(100);
                Console.WriteLine("Server started to wait connecting");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            _ = Task.Run(() => ServerAccept());
        }

        static void ServerAccept()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptCompleted;

            if (!serverSocket.AcceptAsync(acceptEventArg))
            {
                AcceptCompleted(serverSocket, acceptEventArg);
            }
        }

        static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket client = e.AcceptSocket;

                if (client == null || !client.Connected)
                {
                    Console.WriteLine("Error occured while AcceptCompleted");
                    return;
                }

                Console.WriteLine($"New Client connected. {client.RemoteEndPoint}");
                lock (lcs)
                {
                    sockets.Add(client);
                }
                _ = Task.Run(() => StartReceive(client));
                _ = Task.Run(() => ProcessMessage());

                ServerAccept();
            }
        }

        static void StartReceive(Socket client)
        {
            var receiveEventArg = new SocketAsyncEventArgs();
            receiveEventArg.SetBuffer(new byte[4096], 0, 4096); // 버퍼 초기화
            if (receiveEventArg.Buffer == null)
            {
                Console.WriteLine("Buffer is still null after SetBuffer.");
            }
            receiveEventArg.UserToken = client;
            receiveEventArg.Completed += ReceiveCompleted;

            if (!client.ReceiveAsync(receiveEventArg))
            {
                ReceiveCompleted(client, receiveEventArg);
            }
        }

        static void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.Buffer == null)
            {
                Console.WriteLine("Error: Buffer is null. Ensure SetBuffer is called.");
                return;
            }

            Socket client = (Socket)e.UserToken;

            if (e.SocketError == SocketError.Success)
            {
                if (e.BytesTransferred == 0)
                {
                    Console.WriteLine($"{client.RemoteEndPoint} disconnected");
                    lock (lcs)
                    {
                        sockets.Remove(client);
                    }
                    client.Dispose();
                    return;
                }

                if (e.BytesTransferred > 0)
                {
                    Console.WriteLine($"Bytes Transferred: {e.BytesTransferred}");
                    Console.WriteLine($"Buffer Length: {e.Buffer.Length}");

                    for (int i = 0; i < e.BytesTransferred; ++i)
                    {
                        lock (bcs)
                        {
                            buffer.Add(e.Buffer[i]);
                        }
                    }
                    StartReceive(client);
                }
            }
            else
            {
                Console.WriteLine($"Socket error: {e.SocketError}");
                lock (lcs)
                {
                    sockets.Remove(client);
                }
                client.Dispose();
            }
        }

        static async Task ProcessMessage()
        {
            byte[] data = new byte[4096];
            if (buffer.Count > Message.HEADERLENGTH) // 값 확인만 할거라 lock 안 걸었음
            {
                for (int i = 0; i < Message.HEADERLENGTH; ++i)
                {
                    data[i] = buffer[i];
                }

                MessageHeader header = Message.DeserializeHeader(data);
                if (!header.IsHeader())
                {
                    Console.WriteLine("Something very wrong");
                }
                if (header.Protocol == Message.TEXT)
                {
                    if (buffer.Count > header.TotalLength)
                    {
                        lock (bcs)
                        {
                            data = buffer.Take(header.TotalLength).ToArray();
                        }
                        var message = Message.Deserialize(data);
                        Console.WriteLine($"from a: {message.Payload.Text}");
                        _ = BroadCast(message.Payload.Text);
                    }
                }
            }
        }

        static async Task BroadCast(string text)
        {
            Message message = new(Message.TEXT, text.Length, 0, text);
            List<Socket> clients;
            lock (lcs)
            {
                clients = new List<Socket>(sockets);
            }
                foreach (var c in clients)
                {
                    c.SendAsync(Message.Serialize(message));
                }
        }
    }
}

