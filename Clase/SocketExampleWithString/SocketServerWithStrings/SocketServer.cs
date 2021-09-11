using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SocketServerWithStrings
{
    public class SocketServer
    {
        private readonly int _portNumber = 9000;
        private readonly int _backLog = 1;

        public void StartListening()
        {
            // 127.0.0.1:9000
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, _portNumber);

            // Crear Socket TCP/IP
            Socket listener = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Asociar el socket con la direccion ip y el puerto
            listener.Bind(endPoint);
            // escuchar por conexiones entrantes
            listener.Listen(_backLog);

            while (true)
            {
                Console.WriteLine("Esperando por conexiones....");
                var handler = listener.Accept();
                Thread threadProcessor = new Thread(() => HandleReceivedClients(handler));
                threadProcessor.Start();
                Thread threadSendProcessor = new Thread(() => SendMessageFromServer(handler));
                threadSendProcessor.Start();
            }

        }

        private void HandleReceivedClients(Socket handler)
        {
            byte[] bytes = new byte[1024];

            string data = null;

            while (true)
            {
                int bytesRec = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                Console.WriteLine("Texto recibido : {0}", data);

                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

            handler.Shutdown(SocketShutdown.Both);         

            handler.Close();
        }

        private void SendMessageFromServer(Socket socket)
        {
            // Echo the data back to the client.  
            //byte[] msg = Encoding.ASCII.GetBytes("mensaje desde el server...");
            //socket.Send(msg);
            while (true)
            {
                Console.WriteLine("Ingrese mensaje a enviar");
                var msg = Console.ReadLine();
                if (msg == "exit") break;
                socket.Send(Encoding.ASCII.GetBytes(msg));
            }
        }

    }
}
