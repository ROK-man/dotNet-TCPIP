using System.Net;
using System.Net.Sockets;
using System.Text;
using Data;

namespace ChattingServer
{
    internal class Program
    {
        static Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static List<Socket> sockets = new();
        static readonly object cs = new();
        static Queue<byte> dataQ = new();

        // 서버 동작 처리
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
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
                    Console.WriteLine($"Current connected clients: {sockets.Count}");
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

        // 서버 리스닝 시작
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

        // 클라이언트 접속 시작
        static void ServerAccept()
        {
            var acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += AcceptCompleted;

            if (!serverSocket.AcceptAsync(acceptEventArg))
            {
                AcceptCompleted(serverSocket, acceptEventArg);
            }
        }

        // 클라이언트 접속 처리
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
                lock (cs)
                {
                    sockets.Add(client);
                }
                _ = Task.Run(() => StartReceive(client));
                _ = Task.Run(() => ProcessMessageAsync());

                ServerAccept();
            }
        }

        // 클라이언트 메시지 수신 시작
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

        // 클라이언트 메시지 수신 처리
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
                    lock (cs)
                    {
                        sockets.Remove(client);
                    }
                    client.Dispose();
                    return;
                }

                if (e.BytesTransferred > 0)
                {
                    for (int i = 0; i < e.BytesTransferred; ++i)
                    {
                        lock (cs)
                        {
                            dataQ.Enqueue(e.Buffer[i]);
                        }
                    }
                    StartReceive(client);
                }
            }
            else
            {
                Console.WriteLine($"Socket error: {e.SocketError}");
                lock (cs)
                {
                    sockets.Remove(client);
                }
                client.Dispose();
            }
        }

        static async Task ProcessMessageAsync()
        {
            while (true)
            {
                // 헤더 길이만큼 데이터를 처리할 수 있는지 확인
                if (dataQ.Count > Message.HEADERLENGTH) // 값 확인만 할거라 lock 안 걸었음
                {
                    // 1. buffer에서 HEADERLENGTH만큼 데이터를 가져옴
                    byte[] buffer = new byte[4096];
                    lock (cs)
                    {
                        for (int i = 0; i < Message.HEADERLENGTH; ++i)
                        {
                            buffer[i] = dataQ.Dequeue();
                        }
                    }

                    // 2. 헤더 파싱
                    Message message = Message.ParseByteToHeader(buffer);

                    // 헤더 검증 실패시
                    if (!message.IsHeaderVaild())
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Something very wrong");
                        lock (cs)
                        {
                            dataQ.Clear();
                        }
                        continue;
                    }

                    // 3. 메시지의 총 길이를 확인하고, 메시지를 처리
                    if (message.Header.Protocol == Message.TEXT)
                    {
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
                            if(temp > 10)
                            {
                                break;
                            }
                        }
                        if (dataQ.Count >= message.Header.TotalLength - Message.HEADERLENGTH)
                        {
                            lock (cs)
                            {
                                for (int i = Message.HEADERLENGTH; i < message.Header.TotalLength; ++i)
                                {
                                    buffer[i] = dataQ.Dequeue();
                                }
                            }
                            message = Message.ParseByte(buffer);
                            Console.WriteLine($"from {message.Header.UserID}: {message.Payload.Text}");

                            _ = Task.Run(() => BroadCast(message));
                        }
                    }
                    // 다른 프로토콜 처리
                    else
                    {
                        continue;
                    }
                }
            }
        }


        static void BroadCast(Message message)
        {
            lock (cs)
            {
                List<Socket> clients;
                lock (cs)
                {
                    clients = new List<Socket>(sockets);
                }
                foreach (var c in clients)
                {
                    _ = c.SendAsync(message.ToBytes());
                }
            }
        }
    }
}

