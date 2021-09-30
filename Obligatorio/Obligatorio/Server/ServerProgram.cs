using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DTOs;
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
            StartServer();
            int command = -1;
            while (!_exit)
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

                            var socketTrampa = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                ProtocolType.Tcp);
                            socketTrampa.Connect("127.0.0.1", 20000);
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
                            Console.WriteLine("Agregando juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            GameDTO game = serverHandler.ReceiveGame(bufferData);
                            AddGame(game, clientSocket);
                            break;
                        case CommandConstants.SendGameCover:
                            Console.WriteLine("Agregando imagen a juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            serverHandler.AddCoverGame(clientSocket, bufferData);
                            Console.WriteLine("Imagen agregada de manera exitosa");
                            break;
                        case CommandConstants.GetGames:
                            Console.WriteLine("Mostrando juegos");
                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetAllGames()));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.DeleteGame:
                            Console.WriteLine("Borrando juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int id = serverHandler.ReceiveId(bufferData);
                            DeleteGame(id, clientSocket);
                            break;
                        case CommandConstants.ModifyGame:
                            Console.WriteLine("Modificando juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            GameDTO modifyingGame = serverHandler.ReceiveGameForModifying(bufferData);
                            ModifyGame(modifyingGame, clientSocket);
                            break;
                        case CommandConstants.QualifyGame:
                            Console.WriteLine("Calificando juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            ReviewDTO gameReview = serverHandler.ReceiveQualification(bufferData);
                            QualifyGame(gameReview, clientSocket);
                            break;
                        case CommandConstants.ViewDetail:
                            Console.WriteLine("Viendo detalle de juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int gameId = serverHandler.ReceiveId(bufferData);

                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetGameDetail(gameId)));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.SearchForGame:
                            Console.WriteLine("Buscando juegos");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            var data = serverHandler.ReceiveSearchTerms(bufferData);
                            string result = gameService.GetAllByQuery(data);

                            clientSocket.Send(Encoding.UTF8.GetBytes(result));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.ViewBoughtGames:
                            Console.WriteLine("Viendo juegos comprados");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            string username = serverHandler.ReceiveOwnerName(bufferData);
                            string aux = gameService.GetAllBoughtGames(username);

                            clientSocket.Send(Encoding.UTF8.GetBytes(aux));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.BuyGame:
                            Console.WriteLine("Usuario adquiriendo juego");
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            Tuple<int,string> purchaseData = serverHandler.RecieveBuyerInfo(bufferData);
                            BuyGame(purchaseData, clientSocket);
                            break;
                    }
                }
                catch (Exception se)
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
                        throw new Exception("Se ha desconectado el cliente.");
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

        private static void AddGame(GameDTO game, Socket clientSocket)
        {
            try
            {
                gameService.AddGame(game);
                clientSocket.Send(Encoding.UTF8.GetBytes(game.Name + "\n"));
                Console.WriteLine("Juego agregado: " + game.Name);
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientSocket.Send(Encoding.UTF8.GetBytes(e.Message));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
        }

        private static void DeleteGame(int id, Socket clientSocket)
        {
            try
            {
                gameService.DeleteGame(id);
                clientSocket.Send(Encoding.UTF8.GetBytes("Juego con id: " + id + " borrado \n"));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientSocket.Send(Encoding.UTF8.GetBytes(e.Message));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
        }

        private static void ModifyGame(GameDTO game, Socket clientSocket)
        {
            try
            {
                gameService.ModifyGame(game.Id, game);
                clientSocket.Send(Encoding.UTF8.GetBytes("Juego modificado: " + game.Name + "\n"));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientSocket.Send(Encoding.UTF8.GetBytes(e.Message));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
        }

        private static void QualifyGame(ReviewDTO gameReview, Socket clientSocket)
        {
            try
            {
                gameService.QualifyGame(gameReview);
                clientSocket.Send(
                    Encoding.UTF8.GetBytes("Juego con id: " + gameReview.GameId + " calificado.\n"));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientSocket.Send(Encoding.UTF8.GetBytes(e.Message));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
        }

        private static void BuyGame(Tuple<int,string> purchaseData, Socket clientSocket)
        {
            try
            {
                gameService.BuyGame(purchaseData);

                clientSocket.Send(Encoding.UTF8.GetBytes("Juego adquirido de manera exitosa"));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                clientSocket.Send(Encoding.UTF8.GetBytes(e.Message));
                clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
            }
        }

        static int PrintMenu()
        {
            Console.WriteLine("Server >>>");
            Console.WriteLine("----------------");
            Console.WriteLine();
            Console.WriteLine("0 - Salir");
            string command = Console.ReadLine();
            try
            {
                return Int32.Parse(command);
            }
            catch (Exception)
            {
                Console.WriteLine("Por favor seleccione una de las opciones ofrecidas.");
                return -1;
            }
        }
    }
}