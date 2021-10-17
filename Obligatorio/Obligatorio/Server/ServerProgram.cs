using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Extensions.Configuration;
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
        private IConfiguration _configuration;
        private static string ip;
        private static int port;
        private static int backlog;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            ObtainConfiguration();
            var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketServer.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            socketServer.Listen(backlog);
            serverHandler = new ServerHandler(socketServer);
            GameRepository gameRepository = new GameRepository();
            gameService = new GameService(gameRepository);

            // abro un hilo
            Task.Run(() => ListenForConnections(socketServer, gameService));

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
                            socketTrampa.Connect(ip, port);
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

        private static void ObtainConfiguration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            var config = builder.Build();
            ip = config["Ip"];
            port = Int32.Parse(config["Port"]);
            backlog = Int32.Parse(config["Backlog"]);
        }
        
        private static void ListenForConnections(Socket socketServer, GameService gameService)
        {
            while (!_exit)
            {
                try
                {
                    var clientConnected = socketServer.Accept();
                    _clients.Add(clientConnected);
                    // abro un hilo por cliente
                    Task.Run(() => HandleClient(clientConnected, gameService));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }

            Console.WriteLine("Exiting....");
        }

        private static async Task HandleClient(Socket clientSocket, GameService gameService)
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
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            GameDTO game = serverHandler.ReceiveGame(bufferData);
                            await AddGame(game, clientSocket);
                            break;
                        case CommandConstants.SendGameCover:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            serverHandler.AddCoverGame(clientSocket, bufferData);
                            break;
                        case CommandConstants.GetGames:
                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetAllGames()));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.DeleteGame:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int id = serverHandler.ReceiveId(bufferData);
                            await DeleteGame(id, clientSocket);
                            break;
                        case CommandConstants.ModifyGame:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            GameDTO modifyingGame = serverHandler.ReceiveGameForModifying(bufferData);
                            await ModifyGame(modifyingGame, clientSocket);
                            break;
                        case CommandConstants.QualifyGame:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            ReviewDTO gameReview = serverHandler.ReceiveQualification(bufferData);
                            await QualifyGame(gameReview, clientSocket);
                            break;
                        case CommandConstants.ViewDetail:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            int gameId = serverHandler.ReceiveId(bufferData);

                            clientSocket.Send(Encoding.UTF8.GetBytes(gameService.GetGameDetail(gameId)));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.SearchForGame:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            var data = serverHandler.ReceiveSearchTerms(bufferData);
                            string result = gameService.GetAllByQuery(data);

                            clientSocket.Send(Encoding.UTF8.GetBytes(result));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.ViewBoughtGames:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            string username = serverHandler.ReceiveOwnerName(bufferData);
                            string aux = gameService.GetAllBoughtGames(username);

                            clientSocket.Send(Encoding.UTF8.GetBytes(aux));
                            clientSocket.Send(Encoding.UTF8.GetBytes("<EOF>"));
                            break;
                        case CommandConstants.BuyGame:
                            bufferData = new byte[header.IDataLength];
                            ReceiveData(clientSocket, header.IDataLength, bufferData);

                            Tuple<int,string> purchaseData = serverHandler.RecieveBuyerInfo(bufferData);
                            await BuyGame(purchaseData, clientSocket);
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
                        throw new Exception("Se ha desconectado el cliente. \n");
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

        private static async Task AddGame(GameDTO game, Socket clientSocket)
        {
            try
            {
                gameService.AddGame(game);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Juego agregado: " + game.Name + "\n"), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task DeleteGame(int id, Socket clientSocket)
        {
            try
            {
                gameService.DeleteGame(id);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Juego con id: " + id + " borrado \n"), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task ModifyGame(GameDTO game, Socket clientSocket)
        {
            try
            {
                gameService.ModifyGame(game.Id, game);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Juego modificado: " + game.Name + "\n"), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task QualifyGame(ReviewDTO gameReview, Socket clientSocket)
        {
            try
            {
                gameService.QualifyGame(gameReview);
                await clientSocket.SendAsync(
                    Encoding.UTF8.GetBytes("Juego con id: " + gameReview.GameId + " calificado.\n"), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task BuyGame(Tuple<int,string> purchaseData, Socket clientSocket)
        {
            try
            {
                gameService.BuyGame(purchaseData);

                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("Juego adquirido de manera exitosa. \n"), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await clientSocket.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
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