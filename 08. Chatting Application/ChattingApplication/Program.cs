using System.Net;
using System.Net.Sockets;
using System.Text;
using Data;

namespace ChattingApplication
{
    internal class Program
    {
        private static readonly Queue<Message> messageQ = new();
        static readonly object cs = new();


        static readonly Queue<byte> dataQ = new();

        static async Task Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
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
            Console.WriteLine($"Server Connected {client.RemoteEndPoint}");
            _ = Task.Run(() => Send(client));
            _ = Task.Run(() => ProcessDataAsync(client));
            _ = Task.Run(() => Receive(client));

            while (true)
            {
                var input = Console.ReadLine() ?? "";
                if (input.Equals("exit"))
                {
                    client.Close();
                    break;
                }
                else if (input.Length == 0)
                {
                    for (int i = 0; i < 100; ++i)
                    {

                        Message message = new(Message.TEXT, 0, "ㅋㅋㅋzzz");
                        lock (cs)
                        {
                            messageQ.Enqueue(message);
                        }
                    }
                }
                else if (input.Length > 0)
                {
                    if (input.Equals("check"))
                    {
                        Console.WriteLine(dataQ.Count);
                    }
                    if (input.Equals("")) input = "abcㅁㄴㅇ";
                    Message message = new(Message.TEXT, 0, input.ToString());
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
            byte[] buffer = new byte[4096];
            while (true)
            {
                try
                {
                    if (client.Connected)
                    {
                        var receiveBytes = await client.ReceiveAsync(buffer);
                        Console.WriteLine("1");

                        // 연결이 정상적으로 종료된 경우
                        if (receiveBytes == 0)
                        {
                            Console.WriteLine("Connection closed by server.");
                            break;
                        }

                        // 받은 데이터를 큐에 추가
                        for (int i = 0; i < receiveBytes; ++i)
                        {
                            lock (cs)
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


        static async Task ProcessDataAsync(Socket client)
        {
            byte[] buffer = new byte[4096];
            try
            {
                while (client.Connected)
                {
                    if (!client.Connected)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("??");
                        return;
                    }

                    if (dataQ.Count > Message.HEADERLENGTH)
                    {
                        lock (cs)
                        {
                            for (int i = 0; i < Message.HEADERLENGTH; ++i)
                            {
                                buffer[i] = dataQ.Dequeue();
                            }
                        }

                        Message message = Message.ParseByteToHeader(buffer);

                        if (!message.IsHeaderVaild())
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Fail to ProcessData...");
                            lock (cs)
                            {
                                lock (cs)
                                {
                                    while (dataQ.Count != 0)
                                    {
                                        var m = dataQ.Dequeue();
                                        Console.Write($"{m}\t");
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Header right");
                        }

                        int temp = 0;
                        while (!(dataQ.Count < message.Header.TotalLength - Message.HEADERLENGTH))
                        {
                            lock (cs)
                            {
                                if (dataQ.Count >= message.Header.TotalLength - Message.HEADERLENGTH)
                                {
                                    break;
                                }
                            }
                            await Task.Delay(10); // 10ms 대기
                            ++temp;
                            if (temp > 10)
                            {
                                break;
                            }
                        }

                        if (message.Header.Protocol == Message.TEXT)
                        {
                            for (int i = Message.HEADERLENGTH; i < message.Header.TotalLength; ++i)
                            {
                                buffer[i] = dataQ.Dequeue();
                            }

                            Message m = Message.ParseByte(buffer);

                            Console.WriteLine("Server: " + m.Payload.Text);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
