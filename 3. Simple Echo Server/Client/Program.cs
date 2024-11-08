using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sub
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipEndPoint = new(IPAddress.Parse("127.0.0.1"), 25000);

            Socket Client = new(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Client.Connect(ipEndPoint);
            Console.WriteLine("Server Connected, serevr IP:" + ipEndPoint.Address.ToString());
            Console.WriteLine("If you want to quit, send exit");

            while (true)
            {
                // Send message.
                Console.Write("Input: ");
                var message = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "null input";
                }
                var messageBytes = Encoding.UTF8.GetBytes(message);
                _ = Client.Send(messageBytes, SocketFlags.None);

                // Receive ack.
                var buffer = new byte[1_024];
                var received = Client.Receive(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);
                Console.WriteLine(
                    $"Socket client received: \"{response}\"");
                if (response == "exit")
                {
                    Console.WriteLine("Closing Connect");
                    break;
                }
            }

            Client.Shutdown(SocketShutdown.Both);
            Client.Close();
        }
    }
}
