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

            byte[] buffer = new byte[65536];
            byte[] sendBuffer;

            while (true)
            {
                var message = Console.ReadLine();
                if (string.IsNullOrEmpty(message))
                    message = "a";

                if (message == "check")
                {
                    int bytesRead = await client.ReceiveAsync(buffer, SocketFlags.None);
                    var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(bytesRead + "words received");
                    Console.WriteLine("Client received: " + response);

                    if (response.ToLower() == "exit")
                        break;
                    continue;
                }
                sendBuffer = Encoding.UTF8.GetBytes(message);   
                await client.SendAsync(sendBuffer, SocketFlags.None);
            }

            Console.WriteLine("Disconnecting");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
