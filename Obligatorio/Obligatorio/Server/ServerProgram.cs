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
        static List<TcpClient> _clients = new List<TcpClient>();
        static GameService gameService;
        static ServerHandler serverHandler;
        static byte[] bufferData;
        private IConfiguration _configuration;
        private static string ip;
        private static int port;
        private static int backlog;

        private static TcpClient tcpClient;
        private static TcpListener tcpListener;
        private static IPEndPoint listenerEndPoint;
        private static IPEndPoint clientEndPoint;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            ObtainConfiguration();

            listenerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpListener = new TcpListener(listenerEndPoint);
            tcpListener.Start();
            // abro un hilo
            Task.Run(() => ListenForConnectionsAsync(tcpListener, gameService));

            //var socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socketServer.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            //socketServer.Listen(backlog);
            clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            tcpClient = new TcpClient(clientEndPoint);
            serverHandler = new ServerHandler(tcpClient);
            GameRepository gameRepository = new GameRepository();
            gameService = new GameService(gameRepository);

            

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
                            tcpListener.Server.Close(0);
                            foreach (var client in _clients)
                            {
                                client.Client.Shutdown(SocketShutdown.Both);
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
        
        private static async Task ListenForConnectionsAsync(TcpListener tcpListener, GameService gameService)
        {
            while (!_exit)
            {
                try
                {
                    var connectedClient = await tcpListener.AcceptTcpClientAsync();
                    _clients.Add(connectedClient);
                    // abro un hilo por cliente
                    Task.Run(() => HandleClient(connectedClient, gameService));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }

            Console.WriteLine("Exiting....");
        }

        private static async Task HandleClient(TcpClient tcpClient, GameService gameService)
        {
            while (!_exit)
            {
                var headerLength = HeaderConstants.Request.Length + HeaderConstants.CommandLength +
                                   HeaderConstants.DataLength;
                var buffer = new byte[headerLength];
                try
                {
                    await ReceiveDataAsync(tcpClient, headerLength, buffer);
                    var header = new Header();
                    header.DecodeData(buffer);
                    switch (header.ICommand)
                    {
                        case CommandConstants.Login:
                        case CommandConstants.AddGame:
                            await AddGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.SendGameCover:
                            await SendGameCoverAsync(header, tcpClient);
                            break;
                        case CommandConstants.GetGames:
                            await GetGamesAsync(header, tcpClient);
                            break;
                        case CommandConstants.DeleteGame:
                            await DeleteGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.ModifyGame:
                            await ModifyGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.QualifyGame:
                            await QualifyGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.ViewDetail:
                            await ViewGameDetailAsync(header, tcpClient);
                            break;
                        case CommandConstants.SearchForGame:
                            await SearchForGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.ViewBoughtGames:
                            await ViewBoughtGamesAsync(header, tcpClient);
                            break;
                        case CommandConstants.BuyGame:
                            await BuyGameAsync(header, tcpClient);
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


        private static async Task ReceiveDataAsync(TcpClient client, int Length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < Length)
            {
                try
                {
                    var localRecv = await client.Client.ReceiveAsync(buffer, SocketFlags.None);
                    if (localRecv == 0)
                    {
                        client.Client.Shutdown(SocketShutdown.Both);
                        client.Client.Close();
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

        private static async Task SendGameCoverAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            await serverHandler.AddCoverGame(bufferData);
        }

        private static async Task GetGamesAsync(Header header, TcpClient client)
        {
            await client.Client.SendAsync(Encoding.UTF8.GetBytes(gameService.GetAllGames()), SocketFlags.None);
            await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
        }

        private static async Task AddGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            GameDTO game = serverHandler.ReceiveGame(bufferData);

            try
            {
                gameService.AddGame(game);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("Juego agregado: " + game.Name + "\n"), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task DeleteGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            int id = serverHandler.ReceiveId(bufferData);

            try
            {
                gameService.DeleteGame(id);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("Juego con id: " + id + " borrado \n"), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task ModifyGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            GameDTO game = serverHandler.ReceiveGameForModifying(bufferData);

            try
            {
                gameService.ModifyGame(game.Id, game);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("Juego modificado: " + game.Name + "\n"), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task QualifyGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            ReviewDTO gameReview = serverHandler.ReceiveQualification(bufferData);

            try
            {
                gameService.QualifyGame(gameReview);
                await client.Client.SendAsync(
                    Encoding.UTF8.GetBytes("Juego con id: " + gameReview.GameId + " calificado.\n"), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
        }

        private static async Task ViewGameDetailAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            int gameId = serverHandler.ReceiveId(bufferData);

            await client.Client.SendAsync(Encoding.UTF8.GetBytes(gameService.GetGameDetail(gameId)), SocketFlags.None);
            await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
        }

        private static async Task SearchForGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            var data = serverHandler.ReceiveSearchTerms(bufferData);
            string result = gameService.GetAllByQuery(data);

            await client.Client.SendAsync(Encoding.UTF8.GetBytes(result), SocketFlags.None);
            await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
        }
        private static async Task ViewBoughtGamesAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            string username = serverHandler.ReceiveOwnerName(bufferData);
            string aux = gameService.GetAllBoughtGames(username);

            await client.Client.SendAsync(Encoding.UTF8.GetBytes(aux), SocketFlags.None);
            await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
        }

        private static async Task BuyGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            Tuple<int, string> purchaseData = serverHandler.RecieveBuyerInfo(bufferData);

            try
            {
                gameService.BuyGame(purchaseData);

                await client.Client.SendAsync(Encoding.UTF8.GetBytes("Juego adquirido de manera exitosa. \n"), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes(e.Message), SocketFlags.None);
                await client.Client.SendAsync(Encoding.UTF8.GetBytes("<EOF>"), SocketFlags.None);
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