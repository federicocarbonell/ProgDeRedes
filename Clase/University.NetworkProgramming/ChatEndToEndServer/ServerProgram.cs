using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ChatEndToEndServer
{
    class ServerProgram
    {
        private const int ServerPort = 6000;
        private const int HeaderLength = 4;

        static void Main(string[] args)
        {
            Socket serverSocket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);
            IPEndPoint serverEndPoint = new IPEndPoint(
                IPAddress.Parse("127.0.0.1"), ServerPort);

            serverSocket.Bind(serverEndPoint);
            serverSocket.Listen(1);
            Console.WriteLine($"The server can start listening on port {ServerPort}");

            Socket clientSocket = serverSocket.Accept();

            // new Thread(() => ListenForMessages(clientSocket)).Start();
            while (true)
            {
                //string message = Console.ReadLine();
                //byte[] data = Encoding.UTF8.GetBytes(message);
                //clientSocket.Send(data);
                ListenForMessages(clientSocket);
            }
        }

        static void ListenForMessages(Socket clientSocket)
        {

            byte[] dataLength = new byte[HeaderLength];
            int receivedTotal = 0;
            try
            {
                while (receivedTotal < HeaderLength)
                {
                    var received = 0;
                    received += clientSocket.Receive(dataLength, receivedTotal, HeaderLength - receivedTotal, SocketFlags.None);
                    if (received == 0)
                    {
                        throw new SocketException();
                    }
                    receivedTotal += received;
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                {
                    Thread.Sleep(100);
                }
            }

            int length = BitConverter.ToInt32(dataLength);

            byte[] data = new byte[length];

            receivedTotal = 0;
            while (receivedTotal < length)
            {
                var received = 0;
                received += clientSocket.Receive(data, receivedTotal, length - receivedTotal, SocketFlags.None);
                receivedTotal += received;
            }

            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Client says: {message}");
        }

    }
}
