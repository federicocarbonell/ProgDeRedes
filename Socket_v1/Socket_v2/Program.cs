using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerSocketSimpleExample
{
    class Server
    {
        static void Main(string[] args)
        {
            int n = 0;
            int max = 3;

            int port = 9000; // entre 6000 y 9000 para que sea cnocido y sea poco probabale que este en uso
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, port);
            Socket listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            listener.Bind(endpoint);

            listener.Listen(10);
            Console.WriteLine("Escuchando...");

            while (true)
            {
                if (n < max)
                {
                    listener.Accept();
                    new Thread(() => HacerAlgo(n)).Start();
                    n++;
                }

            }
        }

        public static void HacerAlgo(int cliente)
        {
            Console.WriteLine("cliente conectado " + cliente.ToString());
        }

    }
}