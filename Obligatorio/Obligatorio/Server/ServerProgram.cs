using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Client.DTOs;
using ProtocolLibrary;
using StateServices;

namespace Server
{
    public class ServerProgram
    {
        static bool _exit = false;
        static List<Socket> _clients = new List<Socket>();
        static GameService gameService;
        static ServerHandler serverHandler;
        static byte[] bufferData;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            serverHandler = new ServerHandler();
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);
            GameRepository gameRepository = new GameRepository();
            gameService = new GameService(gameRepository);
            var threadServer = new Thread(() => ListenForConnections(socketServer, gameService));
            threadServer.Start();
            //serverHandler = new ServerHandler();
            StartServer();
            int command = -1;
            do
            {
                try
                {
                    command = PrintMenu();
                    switch (command)
                    {
                        case 1:
                            CreateUser();
                            break;
                        case 2:
                            UpdateUser();
                            break;
                        case 3:
                            DeleteUser();
                            break;
                        case 4:
                            ActiveUsers();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unexpected exception: {0}", ex.Message);
                }
                finally
                {
                    Console.WriteLine();
                }
            } while (command != 0);
        }

        static void StartServer()
        {

        }

        private static void ActiveUsers()
        {
            throw new NotImplementedException();
        }

        private static void DeleteUser()
        {
            throw new NotImplementedException();
        }

        private static void UpdateUser()
        {
            throw new NotImplementedException();
        }

        private static void CreateUser()
        {
            throw new NotImplementedException();
        }

        private static void ListenForConnections(Socket socketServer, GameService gameService)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    var threacClient = new Thread(() => HandleClient(clientConnected, gameService));
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

        private static void HandleClient(Socket clientSocket, GameService gameService)
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
                        bufferData = new byte[header.IDataLength];
                        ReceiveData(clientSocket, header.IDataLength, bufferData);

                        GameDTO game = serverHandler.ReceiveGame(bufferData);
                        AddGame(game);
                        Console.WriteLine("Game added: " + game.Name);
                        break;
                    case CommandConstants.GetGames:
                        Console.WriteLine("Not implemented yet");
                        //Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                        break;
                    case CommandConstants.DeleteGame:
                        //Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                        Console.WriteLine("Deleting game");
                        bufferData = new byte[header.IDataLength];
                        ReceiveData(clientSocket, header.IDataLength, bufferData);

                        int id = serverHandler.ReceiveId(bufferData);
                        DeleteGame(id);
                        Console.WriteLine("Game with id: " + id + " deleted");
                        break;
                    case CommandConstants.ModifyGame:
                        Console.WriteLine("Not implemented yet");
                        //Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                        break;
                    case CommandConstants.QualifyGame:
                        Console.WriteLine("Not implemented yet");
                        //Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
                        break;
                    case CommandConstants.ViewDetail:
                        Console.WriteLine("Not implemented yet");
                        //Console.WriteLine("Message received: " + Encoding.UTF8.GetString(bufferData));
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

        private static void AddGame(GameDTO game)
        {
            gameService.AddGame(game);
        }

        private static void DeleteGame(int id)
        {
            gameService.DeleteGame(id);
        }

        static int PrintMenu()
        {
            Console.WriteLine("Server >>>");
            Console.WriteLine("----------------");
            Console.WriteLine();
            Console.WriteLine("1 - Create user");
            Console.WriteLine("2 - Update user");
            Console.WriteLine("3 - Delete user");
            Console.WriteLine("4 - Active users");
            Console.WriteLine("0 - Exit");
            Console.Write("Enter command: ");
            string command = Console.ReadLine();
            try
            {
                return Int32.Parse(command);

            }
            catch (Exception)
            {
                Console.WriteLine("Invalid input.");
                return -1;
            }
        }
    }
}
