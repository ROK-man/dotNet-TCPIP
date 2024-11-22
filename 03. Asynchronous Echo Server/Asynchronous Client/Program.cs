using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Asynchronous_Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000));
            Console.WriteLine("Server connected");

            byte[] receiveBuffer = new byte[65536];
            byte[] sendBuffer;

            Task send;
            Task receive = client.ReceiveAsync(receiveBuffer);

            while (client.Connected)
            {
                var message = Console.ReadLine();
                if (string.IsNullOrEmpty(message))
                    continue;

                sendBuffer = Encoding.UTF8.GetBytes(message);
                send = client.SendAsync(sendBuffer);

                await receive;
                {
                    var received = ((Task<int>)receive).Result;
                    if (received > 0)
                    {
                        var echoMessage = Encoding.UTF8.GetString(receiveBuffer, 0, received);
                        Console.WriteLine("Server Echo: " + echoMessage);

                        if (echoMessage.Equals("exit"))
                        {
                            Console.WriteLine("Disconnecting...");
                            {
                                client.Shutdown(SocketShutdown.Both);
                                client.Close();
                            }
                        }

                        receive = client.ReceiveAsync(receiveBuffer);
                    }
                }
            }

            Console.WriteLine("Disconnected Well");
        }
    }
}
