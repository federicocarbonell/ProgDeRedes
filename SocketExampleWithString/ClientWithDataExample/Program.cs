using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientWithDataExample
{
    class Program
    {
        static void Main(string[] args)
        {
            StartClient();
            Console.WriteLine("Continue...");
            Console.Read();
        }

        public static void StartClient()
        {
            int port = 9000;
            // 127.0.0.1:9000
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Loopback, port);

            Socket sender = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Conectarse desde un socket client
            sender.Connect(ipEndPoint);
            Console.WriteLine("Socket connected to {0}", sender.RemoteEndPoint.ToString());

            //while (true)
            //{

                // enviar mensaje al server
            new Thread(() => Send(sender)).Start();

            // recibir respuesta del server
            //byte[] bytes = new byte[1024];
            //int bytesReceived = sender.Receive(bytes);

            new Thread(() => Receive(sender)).Start();

                // Parsear texto recibido
                //if (bytesReceived > 0)
                //{
                //    Console.WriteLine("Mensaje recibido = {0}",
                //    Encoding.ASCII.GetString(bytes, 0, bytesReceived));
                //}
            //}
        }

        public static void Send(Socket sender)
        {
            while (true)
            {
                Console.WriteLine("Ingrese mensaje a enviar");
                var msg = Console.ReadLine();
                if (msg == "exit") break;
                sender.Send(Encoding.ASCII.GetBytes(msg));
            }
            sender.Shutdown(SocketShutdown.Send);
            sender.Close();

        }

        public static void Receive(Socket sender)
        {
            byte[] bytes = new byte[1024];
            int bytesReceived = sender.Receive(bytes);
            Console.WriteLine("Mensaje recibido = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesReceived));
            while (true)
            {
                int bytesRec = sender.Receive(bytes);
                var data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Texto recibido : {0}", data);

                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }
            sender.Shutdown(SocketShutdown.Receive);
            sender.Close();
        }
    }
}
