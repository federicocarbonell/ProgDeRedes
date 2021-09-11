using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatEndToEndClient
{
    class ClientProgram
    {
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 6000;

        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint clientEndPoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.1"),
                0);
            clientSocket.Bind(clientEndPoint);
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse(ServerIp), ServerPort);
            clientSocket.Connect(serverEndPoint);
            Console.WriteLine("Connected to server");

            new Thread(() => ListenForMessages(clientSocket)).Start();
            while (true)
            {
                string message = Console.ReadLine();
                byte[] data = Encoding.UTF8.GetBytes(message);
                clientSocket.Send(data);
            }
        }

        static void ListenForMessages(Socket clientSocket)
        {
            while (true)
            {
                byte[] data = new byte[256];
                clientSocket.Receive(data);
                string message = Encoding.UTF8.GetString(data);
                Console.WriteLine("Friend says: " + message);
            }
        }
    }
}
