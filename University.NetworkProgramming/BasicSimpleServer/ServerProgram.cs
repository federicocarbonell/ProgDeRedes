using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace BasicSimpleServer
{
    class ServerProgram
    {
        private const int ServerPort = 6000;

        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse("192.168.0.101"), ServerPort);
            // asociar el socket con el endpoint (ip y puerto)
            serverSocket.Bind(serverEndPoint);
            // dejo al socket en modo pasivo escuchando por conexiones
            serverSocket.Listen(100);
            Console.WriteLine($"The server can start listening on port {ServerPort}");
            // bloqueamos el socket hasta que se conecte un cliente
            Socket clientSocket = serverSocket.Accept();
            //Console.WriteLine("Client connected!!!");
            //serverSocket.Close(10);
            Console.ReadLine();
        }
    }
}
