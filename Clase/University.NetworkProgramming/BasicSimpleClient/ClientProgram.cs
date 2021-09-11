using System;
using System.Net;
using System.Net.Sockets;

namespace BasicSimpleClient
{
    class ClientProgram
    {
        private const string ServerIp = "192.168.0.101";
        private const int ServerPort = 6000;

        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(
                IPAddress.Parse("192.168.0.101"),
                0);
            clientSocket.Bind(clientEndPoint);
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse(ServerIp), ServerPort);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to server");
            Console.ReadLine();
        }
    }
}
