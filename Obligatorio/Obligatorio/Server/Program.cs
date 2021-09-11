using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtocolLibrary;
using StateServices;

namespace Server
{
    internal static class Program
    {
        static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();
        static void Main(string[] args)
        {
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);
            var threadServer = new Thread(() => ListenForConnections(socketServer));
            threadServer.Start();

            Console.WriteLine("Bienvenido al Sistema Server");
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("exit -> abandonar el programa");
            Console.WriteLine("Ingrese su opcion: ");
            while (!_exit)
            {
                var userInput = Console.ReadLine();
                switch (userInput)
                {
                    case "exit":
                        _exit = true;
                        socketServer.Close(0);
                        foreach (var client in _clients)
                        {
                            client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                        var socketTrampa = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketTrampa.Connect("127.0.0.1", 20000);
                        break;
                    default:
                        Console.WriteLine("Opcion incorrecta ingresada");
                        break;
                }
            }
        }

        private static void ListenForConnections(Socket socketServer)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    var threacClient = new Thread(() => HandleClient(clientConnected));
                    threacClient.Start();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }
            Console.WriteLine("Exiting....");
        }

        private static void HandleClient(Socket clientSocket)
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                ReceiveData(clientSocket, headerLength, buffer);

                var header = new Header();
                header.DecodeData(buffer);
                switch (header.ICommand)
                {
                    case CommandConstants.Login:
                    case CommandConstants.AddGame:
                        Console.WriteLine("Adding game");
                        var bufferData1 = new byte[header.IDataLength];
                        ReceiveData(clientSocket, header.IDataLength, bufferData1);
                        string gameName = Encoding.UTF8.GetString(bufferData1);
                        AddGame(gameName);
                        Console.WriteLine("Game added: " + gameName);
                        break;
                    case CommandConstants.Message:
                        Console.WriteLine("Will receive message to display...");
                        var bufferData = new byte[header.IDataLength];
                        ReceiveData(clientSocket, header.IDataLength, bufferData);
                        Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                        break;
                }

            }
        }

        private static void ReceiveData(Socket clientSocket, int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = clientSocket.Receive(buffer, iRecv, Length - iRecv, SocketFlags.None);
                    if (localRecv == 0)
                    {
                        clientSocket.Shutdown(SocketShutdown.Both);
                        clientSocket.Close();
                        //throw exception -> se desconecto el cliente remoto
                    }

                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                    return;
                }
            }
        }

        private static void AddGame(String name)
        {
            GameService gameService = new GameService();
            gameService.AddGame(name);
        }

    }
}
