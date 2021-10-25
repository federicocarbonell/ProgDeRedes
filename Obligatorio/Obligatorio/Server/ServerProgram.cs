using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Extensions.Configuration;
using ProtocolLibrary;
using StateServices;
using StateServices.DomainEntities;

namespace Server
{
    public class ServerProgram
    {
        static bool _exit = false;
        static List<TcpClient> _clients = new List<TcpClient>();
        static GameService gameService;
        static AuthenticationService authService;
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

        private static byte[] endMessageBytes = Encoding.UTF8.GetBytes("<EOF>");
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            ObtainConfiguration();

            listenerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpListener = new TcpListener(listenerEndPoint);
            tcpListener.Start();
            // abro un hilo
            GameRepository gameRepository = new GameRepository();
            gameService = new GameService(gameRepository);
            Task.Run(() => ListenForConnectionsAsync(tcpListener));
            UserRepository userRepo = new UserRepository();
            authService = new AuthenticationService(userRepo);

            clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            tcpClient = new TcpClient(clientEndPoint);
            serverHandler = new ServerHandler(tcpClient);

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
                                //aca creo que no esta bien andar usando asi el socket
                                client.Client.Shutdown(SocketShutdown.Both);
                                client.Close();
                            }

                            var socketTrampa = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                                ProtocolType.Tcp);
                            socketTrampa.Connect(ip, port);
                            break;
                        case 1:
                            PrintAddUser();
                            break;
                        case 2:
                            PrintDeleteUser();
                            break;
                        case 3:
                            PrintModifyUser();
                            break;
                        case 4:
                            PrintViewUsers();
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

        private static void PrintViewUsers()
        {
            var users = authService.GetUsers();

            foreach(User u in users.Where(x => x.IsDeleted == false))
            {
                Console.WriteLine($"Id: {u.Id}, username: {u.Username}");
            }                
        }

        private static void PrintModifyUser()
        {
            Console.Write("Modificar usuario con el id: ");
            int id = Int32.Parse(Console.ReadLine());

            Console.Write("Username: ");
            var user = Console.ReadLine();

            Console.Write("Password: ");
            var pass = Console.ReadLine();

            try
            {
                authService.UpdateUser(id, user, pass);
                Console.WriteLine($"Usuario {user} modificado con exito");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }

        private static void PrintDeleteUser()
        {
            Console.Write("Borrar usuario con el id: ");
            int id = Int32.Parse(Console.ReadLine());

            try
            {
                authService.DeleteUser(id);
                Console.WriteLine($"Usuario con el id {id} borrado con exito");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
            }
        }

        private static void PrintAddUser()
        {
            Console.Write("Username: ");
            var user = Console.ReadLine();

            Console.Write("Password: ");
            var pass = Console.ReadLine();

            try
            {
                User u = authService.AddUser(user, pass);
                Console.WriteLine($"Usuario {user} dado de alta con exito");
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
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
        
        private static async Task ListenForConnectionsAsync(TcpListener tcpListener)
        {
            while (!_exit)
            {
                try
                {
                    var connectedClient = await tcpListener.AcceptTcpClientAsync();
                    _clients.Add(connectedClient);
                    // abro un hilo por cliente
                    UserRepository repo = new UserRepository();
                    AuthenticationService clientService = new AuthenticationService(repo);
                    Task.Run(() => HandleClient(connectedClient, clientService));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }

            Console.WriteLine("Exiting....");
        }

        private static async Task HandleClient(TcpClient tcpClient, AuthenticationService clientService)
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
                            await LoginAsync(header, tcpClient, clientService);
                            break;
                        case CommandConstants.AddGame:
                            await AddGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.SendGameCover:
                            await SendGameCoverAsync(header, tcpClient);
                            break;
                        case CommandConstants.GetGames:
                            await GetGamesAsync(tcpClient);
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
                            await ViewBoughtGamesAsync(tcpClient, clientService);
                            break;
                        case CommandConstants.BuyGame:
                            await BuyGameAsync(header, tcpClient, clientService);
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
                    var localRecv = await client.GetStream().ReadAsync(buffer, iRecv, Length);
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

        private static async Task LoginAsync(Header header, TcpClient client, AuthenticationService authService)
        {
            byte[] messageBytes;
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            bool logged = await serverHandler.DoLoginAsync(bufferData, authService);
            if (logged)
            {
                Console.WriteLine($"Usuario autenticado : {authService.GetLoggedUser().Username}");
                messageBytes = Encoding.UTF8.GetBytes("TokenAuth");
            }
            else
            {
                messageBytes = Encoding.UTF8.GetBytes("Wrong credentials");
            }
            await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
        }

        private static async Task SendGameCoverAsync(Header header, TcpClient client)
        {
            //refactorear
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            await serverHandler.AddCoverGameAsync(bufferData);
        }

        private static async Task GetGamesAsync(TcpClient client)
        {
            //refactorear
            var gameBytes = Encoding.UTF8.GetBytes(gameService.GetAllGames());
            await client.GetStream().WriteAsync(gameBytes, 0, gameBytes.Length);
            await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
        }

        private static async Task AddGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            GameDTO game = serverHandler.ReceiveGame(bufferData);

            try
            {
                gameService.AddGame(game);
                //refactorear
                var gameNameBytes = Encoding.UTF8.GetBytes("Juego agregado: " + game.Name + "\n");
                await client.GetStream().WriteAsync(gameNameBytes, 0, gameNameBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                //refactorear
                var messageBytes = Encoding.UTF8.GetBytes(e.Message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
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
                var gameIdBytes = Encoding.UTF8.GetBytes("Juego con id: " + id + " borrado \n");
                await client.GetStream().WriteAsync(gameIdBytes, 0, gameIdBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var messageBytes = Encoding.UTF8.GetBytes(e.Message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
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
                var gameNameBytes = Encoding.UTF8.GetBytes("Juego modificado: " + game.Name + "\n");
                await client.GetStream().WriteAsync(gameNameBytes, 0, gameNameBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var messageBytes = Encoding.UTF8.GetBytes(e.Message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
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
                var gameIdBytes = Encoding.UTF8.GetBytes("Juego con id: " + gameReview.GameId + " calificado \n");
                await client.GetStream().WriteAsync(gameIdBytes, 0, gameIdBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var messageBytes = Encoding.UTF8.GetBytes(e.Message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
        }

        private static async Task ViewGameDetailAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            int gameId = serverHandler.ReceiveId(bufferData);

            var gameDetailBytes = Encoding.UTF8.GetBytes(gameService.GetGameDetail(gameId));
            await client.GetStream().WriteAsync(gameDetailBytes, 0, gameDetailBytes.Length);
            await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
        }

        private static async Task SearchForGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            var data = serverHandler.ReceiveSearchTerms(bufferData);
            var resultBytes = Encoding.UTF8.GetBytes(gameService.GetAllByQuery(data));

            await client.GetStream().WriteAsync(resultBytes, 0, resultBytes.Length);
            await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
        }
        private static async Task ViewBoughtGamesAsync(TcpClient client, AuthenticationService authService)
        {
            var resultBytes = Encoding.UTF8.GetBytes(gameService.GetAllBoughtGames(authService.GetLoggedUser().Username));

            await client.GetStream().WriteAsync(resultBytes, 0, resultBytes.Length);
            await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
        }

        private static async Task BuyGameAsync(Header header, TcpClient client, AuthenticationService authService)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            int gameId = serverHandler.RecieveBuyerInfo(bufferData);

            try
            {
                gameService.BuyGame(gameId, authService.GetLoggedUser().Username);
                var purchaseMessage = Encoding.UTF8.GetBytes("Juego adquirido de manera exitosa. \n");
                await client.GetStream().WriteAsync(purchaseMessage, 0, purchaseMessage.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                var messageBytes = Encoding.UTF8.GetBytes(e.Message);
                await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
                await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
            }
        }

        static int PrintMenu()
        {
            Console.WriteLine("Server >>>");
            Console.WriteLine("----------------");
            Console.WriteLine();
            Console.WriteLine("0 - Salir");
            Console.WriteLine("1 - Crear usuario");
            Console.WriteLine("2 - Borrar usuario");
            Console.WriteLine("3 - Modificar usuario");
            Console.WriteLine("4 - Ver usuarios");
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