﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
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
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20000));
            socketServer.Listen(100);
            serverHandler = new ServerHandler(socketServer);
            GameRepository gameRepository = new GameRepository();
            gameService = new GameService(gameRepository);
            var threadServer = new Thread(() => ListenForConnections(socketServer, gameService));
            threadServer.Start();
            //serverHandler = new ServerHandler();
            StartServer();
            int command = -1;
            while(!_exit)
            {
                try
                {
                    command = PrintMenu();
                    switch (command)
                    {
                        case 0:
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
            }
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
                try
                {
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
                            clientSocket.Send(Encoding.UTF8.GetBytes(game.Name + "\n"));
                            Console.WriteLine("Game added: " + game.Name);
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.SendGameCover:
                            Console.WriteLine("Adding cover");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            serverHandler.AddCoverGame(clientSocket, bufferData);
                            Console.WriteLine("Add cover ok");
                            break;
                        case CommandConstants.GetGames:
                            Console.WriteLine("Showing games");
                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetAllGames()));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.DeleteGame:
                            Console.WriteLine("Deleting game");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int id = serverHandler.ReceiveId(bufferData);
                            DeleteGame(id);
                            clientSocket.Send(Encoding.UTF8.GetBytes("Game with id: " + id + " deleted \n"));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.ModifyGame:
                            Console.WriteLine("Modifying game");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            GameDTO modifyingGame = serverHandler.ReceiveGameForModifying(bufferData);
                            ModifyGame(modifyingGame);
                            clientSocket.Send(Encoding.UTF8.GetBytes("Game added: " + modifyingGame.Name + "\n"));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.QualifyGame:
                            Console.WriteLine("Qualifying game");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            ReviewDTO gameReview = serverHandler.ReceiveQualification(bufferData);
                            QualifyGame(gameReview);
                            clientSocket.Send(Encoding.UTF8.GetBytes("Qualification for game with id: " + gameReview.GameId + "\n"));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.ViewDetail:
                            Console.WriteLine("Viewing game detail");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int gameId = serverHandler.ReceiveId(bufferData);
                        
                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetGameDetail(gameId)));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.SearchForGame:
                            Console.WriteLine("Searching for games");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            var data = serverHandler.ReceiveSearchTerms(bufferData);
                            string result = gameService.GetAllByQuery(data);

                            clientSocket.Send(Encoding.UTF8.GetBytes(result));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));

                            break;
                    }

                } catch(Exception se)
                {
                    Console.WriteLine(se.Message);
                    return;
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
                        throw new Exception("Client has been desconnected.");
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

        private static void ModifyGame(GameDTO game)
        {
            gameService.ModifyGame(game.Id, game);
        }

        private static void QualifyGame(ReviewDTO gameReview)
        {
            gameService.QualifyGame(gameReview);
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
