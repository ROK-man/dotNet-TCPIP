using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace dotNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ipEndPoint = new(IPAddress.Loopback, 25000);
            Socket socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ipEndPoint);
            socket.Listen(1);
            Console.WriteLine("Listening socket is waiting for a connection...");

            var handler = socket.Accept();
            Console.WriteLine("Client connected: " + handler.RemoteEndPoint?.ToString());

            var buffer = new byte[1024];
            int received;
            while ((received = handler.Receive(buffer)) > 0)
            {
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                Console.WriteLine($"Socket server received message: \"{response}\"");

                var echoBytes = Encoding.UTF8.GetBytes(response);
                handler.Send(echoBytes, SocketFlags.None);

                if (response == "exit")
                {
                    Console.WriteLine("Client has sent Closing message");
                    break; // Exit the loop after acknowledgment
                }
            }

            // Close the handler and socket once done
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            socket.Close();
        }
    }
}
