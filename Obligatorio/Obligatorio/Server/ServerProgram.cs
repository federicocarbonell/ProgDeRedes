using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DTOs;
using Microsoft.Extensions.Configuration;
using ProtocolLibrary;
using StateServices;
using StateServices.DomainEntities;
using RabbitMQ.Client;

namespace Server
{
    public class ServerProgram
    {
        static bool _exit = false;
        static List<TcpClient> _clients = new List<TcpClient>();
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

        private static string QueueName = "logsQueue";
        private static IModel channel;

        private static byte[] endMessageBytes = Encoding.UTF8.GetBytes("<EOF>");
        static void Main(string[] args)
        {
            //EMPIEZA CONFIG QUEUE
            var factory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = factory.CreateConnection();
            channel = connection.CreateModel();
            DeclareQueue(channel);
            //TERMINA CONFIG QUEUE
            Console.WriteLine("Starting server");
            ObtainConfiguration();

            listenerEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            tcpListener = new TcpListener(listenerEndPoint);
            tcpListener.Start();
            // abro un hilo
            GameRepository gameRepository = new GameRepository();
            Task.Run(() => ListenForConnectionsAsync(tcpListener));
            UserRepository userRepo = new UserRepository();

            clientEndPoint = new IPEndPoint(IPAddress.Parse(ip), port + 1);
            tcpClient = new TcpClient(clientEndPoint);
            serverHandler = new ServerHandler(tcpClient, tcpListener);

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
                            break;
                        //case 1:
                        //    PrintAddUser();
                        //    break;
                        //case 2:
                        //    PrintDeleteUser();
                        //    break;
                        //case 3:
                        //    PrintModifyUser();
                        //    break;
                        //case 4:
                        //    PrintViewUsers();
                        //    break;
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
            tcpListener.Stop();
            Console.WriteLine("Exiting....");
        }

        // FUNCIONES QUEUE START

        private static void DeclareQueue(IModel channel)
        {
            channel.QueueDeclare(
                queue: QueueName,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        private static void PublishMessage(IModel channel, string message)
        {
            byte[] body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "",
                routingKey: QueueName,
                basicProperties: null,
                body: body);
        }

        // FUNCIONES QUEUE FIN

        //private static void PrintViewUsers()
        //{
        //    var users = authService.GetUsers();

        //    foreach(User u in users.Where(x => x.IsDeleted == false))
        //    {
        //        Console.WriteLine($"Id: {u.Id}, username: {u.Username}");
        //    }                
        //}

        //private static void PrintModifyUser()
        //{
        //    Console.Write("Modificar usuario con el id: ");
        //    int id = Int32.Parse(Console.ReadLine());

        //    Console.Write("Username: ");
        //    var user = Console.ReadLine();

        //    Console.Write("Password: ");
        //    var pass = Console.ReadLine();

        //    try
        //    {
        //        authService.UpdateUser(id, user, pass);
        //        Console.WriteLine($"Usuario {user} modificado con exito");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"{e.Message}");
        //    }
        //}

        //private static void PrintDeleteUser()
        //{
        //    Console.Write("Borrar usuario con el id: ");
        //    int id = Int32.Parse(Console.ReadLine());

        //    try
        //    {
        //        authService.DeleteUser(id);
        //        Console.WriteLine($"Usuario con el id {id} borrado con exito");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"{e.Message}");
        //    }
        //}

        //private static void PrintAddUser()
        //{
        //    Console.Write("Username: ");
        //    var user = Console.ReadLine();

        //    Console.Write("Password: ");
        //    var pass = Console.ReadLine();

        //    try
        //    {
        //        User u = authService.AddUser(user, pass);
        //        Console.WriteLine($"Usuario {user} dado de alta con exito");
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine($"{e.Message}");
        //    }
        //}

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
                    SessionDTO session = new SessionDTO();
                    Task.Run(() => HandleClient(connectedClient, session));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _exit = true;
                }
            }
        }

        private static async Task HandleClient(TcpClient tcpClient, SessionDTO session)
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
                            session = await LoginAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.AddGame:
                            await AddGameAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.SendGameCover:
                            await ReceiveGameCoverAsync(header, tcpClient);
                            break;
                        case CommandConstants.GetGames:
                            await GetGamesAsync(tcpClient, session);
                            break;
                        case CommandConstants.DeleteGame:
                            await DeleteGameAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.ModifyGame:
                            await ModifyGameAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.QualifyGame:
                            await QualifyGameAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.ViewDetail:
                            await ViewGameDetailAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.SearchForGame:
                            await SearchForGameAsync(header, tcpClient);
                            break;
                        case CommandConstants.ViewBoughtGames:
                            await ViewBoughtGamesAsync(tcpClient, session);
                            break;
                        case CommandConstants.BuyGame:
                            await BuyGameAsync(header, tcpClient, session);
                            break;
                        case CommandConstants.DownloadCover:
                            await SendGameConverAsync(header, tcpClient);
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

        private static async Task<SessionDTO> LoginAsync(Header header, TcpClient client, SessionDTO session)
        {
            byte[] messageBytes;
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            session = await serverHandler.DoLoginAsync(bufferData);
            if (session.Logged)
            {
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Logged in");
                Console.WriteLine($"Usuario autenticado : {session.UserLogged}");
                messageBytes = Encoding.UTF8.GetBytes("TokenAuth");
            }
            else
            {
                messageBytes = Encoding.UTF8.GetBytes("Wrong credentials");
            }
            await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
            return session;
        }

        private static async Task ReceiveGameCoverAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            await serverHandler.AddCoverGameAsync(bufferData, client);
        }

        private static async Task GetGamesAsync(TcpClient client, SessionDTO session)
        {
            string games = await serverHandler.GetGamesAsync();
            PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juegos {games} obtenidos");
            Console.WriteLine($"Usuario autenticado : {session.UserLogged}");
            await SendMessage(client, Encoding.UTF8.GetBytes(games));
        }

        private static async Task AddGameAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            GameDTO game = serverHandler.ReceiveGame(bufferData);

            try
            {
                string response = await serverHandler.AddGameAsync(game);
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juego {game.Name} agregado");
                await SendMessage(client, Encoding.UTF8.GetBytes("Juego agregado: " + game.Name + "\n" + response));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task DeleteGameAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            int id = serverHandler.ReceiveId(bufferData);

            try
            {
                bool response = await serverHandler.DeleteGameAsync(id);
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juego {serverHandler.GetGameNameAsync(id)} borrado");
                await SendMessage(client, Encoding.UTF8.GetBytes("Juego con id: " + id + " borrado \n"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task ModifyGameAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            GameDTO game = serverHandler.ReceiveGameForModifying(bufferData);

            try
            {
                var response = await serverHandler.ModifyGameAsync(game.Id, game);
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juego {game.Name} modificado");
                await SendMessage(client, Encoding.UTF8.GetBytes("Juego modificado: " + game.Name + "\n"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task QualifyGameAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            ReviewDTO gameReview = serverHandler.ReceiveQualification(bufferData);

            try
            {
                var response = await serverHandler.QualifyGameAsync(gameReview);
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juego {serverHandler.GetGameNameAsync(gameReview.GameId)} calificado");
                await SendMessage(client, Encoding.UTF8.GetBytes(response));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task ViewGameDetailAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            int gameId = serverHandler.ReceiveId(bufferData);
            string gameDetails = await serverHandler.GetGameDetailAsync(gameId);
            PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Detalle del juego {serverHandler.GetGameNameAsync(gameId)} visto");
            await SendMessage(client, Encoding.UTF8.GetBytes(gameDetails));
        }

        private static async Task SearchForGameAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);

            var data = serverHandler.ReceiveSearchTerms(bufferData);
            var response = await serverHandler.SearchForGameAsync(data);
            await SendMessage(client, Encoding.UTF8.GetBytes(response));
        }

        private static async Task ViewBoughtGamesAsync(TcpClient client, SessionDTO session)
        {
            PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Vio sus juegos comprados");
            var gamesList = await serverHandler.GetAllBoughtGamesAsync(session.UserLogged);
            await SendMessage(client, Encoding.UTF8.GetBytes(gamesList));
        }

        private static async Task BuyGameAsync(Header header, TcpClient client, SessionDTO session)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            int gameId = serverHandler.RecieveBuyerInfo(bufferData);

            try
            {
                await serverHandler.BuyGameAsync(gameId, session.UserLogged);
                PublishMessage(channel, $"Usuario: {session.UserLogged},Accion: Juego {serverHandler.GetGameNameAsync(gameId)} adquirido por el usuario {session.UserLogged}");
                await SendMessage(client, Encoding.UTF8.GetBytes("Juego adquirido de manera exitosa. \n"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task SendGameConverAsync(Header header, TcpClient client)
        {
            bufferData = new byte[header.IDataLength];
            await ReceiveDataAsync(client, header.IDataLength, bufferData);
            int gameId = serverHandler.ReceiveId(bufferData);
            try
            {
                string gameName = await serverHandler.GetGameNameAsync(gameId);
                string path = gameName + ".png";
                string returnMessage = "La carátula se ha enviado correctamente. \n";
                if (!File.Exists(path))
                {
                    path = "Files/NoImage.png";
                    returnMessage = "La carátula solicitada no existe. \n";
                }
                await serverHandler.SendFileAsync(path, client);
                await SendMessage(client, Encoding.UTF8.GetBytes(returnMessage));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                await SendMessage(client, Encoding.UTF8.GetBytes(e.Message));
            }
        }

        private static async Task SendMessage(TcpClient client, byte[] messageBytes)
        {
            await client.GetStream().WriteAsync(messageBytes, 0, messageBytes.Length);
            await client.GetStream().WriteAsync(endMessageBytes, 0, endMessageBytes.Length);
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