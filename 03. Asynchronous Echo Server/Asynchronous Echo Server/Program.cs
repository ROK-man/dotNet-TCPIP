using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Asynchronous_Echo_Server
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // 리스닝 소켓 바인딩 및 리스닝 시작
            Socket listeningSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listeningSocket.Listen(1);

            Console.WriteLine("Server ready to connect");


            // 서버 동작          
            while (true)
            {
                Socket handler = await listeningSocket.AcceptAsync();
                Console.WriteLine($"Client connected : {handler.RemoteEndPoint.ToString()}");
                byte[] buffer = new byte[65536];
                int bytesRead;

                Task receive = handler.ReceiveAsync(buffer, SocketFlags.None);

                while (handler.Connected)
                {
                    if (receive.IsCompleted)
                    {
                        bytesRead = ((Task<int>)receive).Result;
                        if (bytesRead > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            Console.WriteLine(message);
                            var sendBuffer = Encoding.UTF8.GetBytes(message);

                            _ = handler.Send(sendBuffer);
                            receive = handler.ReceiveAsync(buffer, SocketFlags.None);
                        }
                    }
                }

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine("Client disconnected");
            }
        }
    }
}
