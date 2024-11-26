using ChattingServer;
using System.Net;
using System.Net.Sockets;

namespace ChattingApplication
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            while(true)
            {
                Console.WriteLine("s to connect server");
                Console.WriteLine("q to terminate program");
                var input = Console.ReadLine();
                if(input.Equals("q"))
                {
                    break;
                }
                if(input.Equals("s"))
                {
                    await Chatting();
                }
            }
        }

        static async Task Chatting()
        {
            Socket client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await client.ConnectAsync(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25000));

            while(true)
            {
                var input = Console.ReadLine();
                if(input.Equals("exit"))
                {
                    client.Close();
                    break;
                }
                else
                {
                    Message message = new(Message.TEXT, input.Length, 0, input.ToString());
                    client.Send(Message.Serialize(message));
                }
            }
        }
    }
}
