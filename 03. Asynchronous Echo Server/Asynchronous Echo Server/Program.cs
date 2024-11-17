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
            Socket listeningSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listeningSocket.Bind(new IPEndPoint(IPAddress.Loopback, 25000));
            listeningSocket.Listen();

            Console.WriteLine("Server ready to connect");

            while (true)
            {
                Socket handler = await listeningSocket.AcceptAsync();
                Console.WriteLine("Client connected");
                byte[] buffer = new byte[65536];
                int bytesRead;

                while (true)
                {
                    try
                    {
                        bytesRead = await handler.ReceiveAsync(buffer, SocketFlags.None);
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                    finally
                    {

                    }
                    if (bytesRead == 0)
                        break;

                    var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine("Server received: " + response);

                    if (response.ToLower() == "exit")
                    {
                        Console.WriteLine("Client wants to disconnect");
                        break;
                    }

                    var echo = Encoding.UTF8.GetBytes(response.ToUpper());
                    await handler.SendAsync(echo, SocketFlags.None);
                    Console.WriteLine("Server sent: " + response.ToUpper());
                }


                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
                Console.WriteLine("Client disconnected");

                //string input = Console.ReadLine();
                //if (input == "close")
                //{
                //    break;
                //}
            }
            listeningSocket.Close();
            Console.WriteLine("Server closed");
        }
    }
}
