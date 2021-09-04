using System;
using System.Net;
using System.Net.Sockets;

namespace Socket_v1
{
    class Server
    {
        static void Main(string[] args)
        {
            int port = 9000;
            int backlog = 4;
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, port);
            // Crear el socket.
            Socket listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Asociar el socket a un endpoint.
            listener.Bind(endpoint);

            // Ponwe el socket en escucha de las conexiones.
            listener.Listen(backlog);

            Console.WriteLine("Start listening for client... ");
            var n = 0;
            while (n != 3)
            {
                Socket client = listener.Accept();
                n++;
            }
            Console.WriteLine("Client connected to the server....");
            Console.ReadLine();
        }
    }
}
