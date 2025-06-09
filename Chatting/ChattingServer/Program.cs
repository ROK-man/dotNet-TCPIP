using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace ChattingServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 7777);
            Socket socket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Bind(ep);
            socket.Listen(1);

            var s = socket.Accept();

            Console.WriteLine("New client connected" + s.LocalEndPoint.ToString());

            byte[] buffer = new byte[1024];
            int receivedSize = 0;
            while (true)
            {
                receivedSize = s.Receive(buffer);
                var encoded = Encoding.UTF8.GetString(buffer, 0, receivedSize);
                Console.WriteLine(encoded);
            }
        }
    }
}
