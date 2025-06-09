using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace ChattingClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7777));

            string s;
            while (true)
            {
                s = Console.ReadLine();
                if (s == "")
                {
                    socket.Close();
                }

                var decoded = Encoding.UTF8.GetBytes(s);

                socket.Send(decoded);
            }
        }
    }
}
