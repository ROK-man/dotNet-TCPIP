using System.Net;
using System.Net.Sockets;
using Data;

namespace ChattingApplication
{
    internal class Program
    {
        private static readonly Queue<Message> messageQ = new();
        static readonly object cs = new();
        static readonly object ls = new();


        static readonly Queue<byte> dataQ = new();

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("s to connect server");
                Console.WriteLine("q to terminate program");
                var input = Console.ReadLine() ?? "";
                if (input.Equals("q"))
                {
                    break;
                }
                if (input.Equals("s"))
                {
                    await Chatting();
                }
            }
        }

        static async Task Chatting()
        {
            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000));
            _ = Task.Run(() => Send(client));
            _ = Task.Run(() => ProcessData(client));
            _ = Task.Run(() => Receive(client));

            while (true)
            {
                var input = Console.ReadLine() ?? "";
                if (input.Equals("exit"))
                {
                    client.Close();
                    break;
                }
                else if (input.Length > 0)
                {
                    Message message = new(Message.TEXT, input.Length, 0, input.ToString());
                    lock (cs)
                    {
                        messageQ.Enqueue(message);
                    }
                }
            }
        }

        static async void Send(Socket client)
        {
            Message message;
            while (true)
            {
                if (messageQ.Count > 0)
                {
                    lock (cs)
                    {
                        message = messageQ.Dequeue();
                    }

                    await client.SendAsync(message.ToBytes());

                }
            }

        }

        static async void Receive(Socket client)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    if (client.Connected)
                    {
                        var receiveBytes = await client.ReceiveAsync(buffer);

                        // 연결이 정상적으로 종료된 경우
                        if (receiveBytes == 0)
                        {
                            Console.WriteLine("Connection closed by server.");
                            break;
                        }

                        // 받은 데이터를 큐에 추가
                        for (int i = 0; i < receiveBytes; ++i)
                        {
                            lock (ls)
                            {
                                dataQ.Enqueue(buffer[i]);
                            }
                        }
                    }
                }
                catch (SocketException ex)
                {
                    Console.WriteLine($"Socket exception: {ex.Message}");
                    break;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("Socket has been disposed.");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error: {ex.Message}");
                    break;
                }
            }
            Console.WriteLine("Receive loop exited.");
        }


        static void ProcessData(Socket client)
        {
            byte[] buffer = new byte[4096];
            while (client.Connected)
            {
                if (!client.Connected)
                {
                    return;
                }
                if (dataQ.Count > Message.HEADERLENGTH)
                {
                    lock (ls)
                    {
                        for (int i = 0; i < Message.HEADERLENGTH; ++i)
                        {
                            buffer[i] = dataQ.Dequeue();
                        }
                    }

                    MessageHeader header = MessageHeader.ParseByte(buffer);
                    if (header.Protocol == Message.TEXT && header.IsHeader())
                    {
                        for (int i = Message.HEADERLENGTH; i < header.TotalLength; ++i)
                        {
                            buffer[i] = dataQ.Dequeue();
                        }

                        Message m = Message.ParseByte(buffer);

                        Console.WriteLine("Server: " + m.Payload.Text);
                    }
                }
            }
        }
    }
}
