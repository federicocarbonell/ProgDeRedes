using System;
using System.Net;
using System.Net.Sockets;

namespace ClientSocket_v1
{
    class Client
    {
        private const string ServerIPAddress = "127.0.0.1";
        private const int ServerPort = 9000;

        private const string ClientIPAddress = "127.0.0.1";
        private const int ClientPort = 0;

        static void Main(string[] args)
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Parse(ClientIPAddress), ClientPort);
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            clientSocket.Bind(clientEndPoint);

            Console.WriteLine("Trying to connect to server....");

            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(ServerIPAddress), ServerPort);
            clientSocket.Connect(serverEndPoint);

            Console.WriteLine("Connected to server....");
            Console.ReadLine();

        }
    }
}
