using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Client
{
    public class ClientProgram
    {
        static ClientHandler clientHandler;
        static bool connected;
        static bool authenticated;
        private static IConfiguration _configuration;

        static async Task Main(string[] args)
        {
            try
            {
                ObtainConfiguration();
                clientHandler = new ClientHandler(_configuration);
                int command = -1;
                connected = true;
                while (connected)
                {
                    while (! authenticated)
                    {
                        try
                        {
                            command = PrintLogin();
                            if (command == 1)
                            {
                                authenticated = await DoLogin();
                                if(! authenticated)
                                    Console.WriteLine("Error de autenticacion, por favor chequee sus credenciales");
                            }
                            else
                                PrintLogout();
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    try
                    {
                        command = PrintMenu();
                        switch (command)
                        {
                            case 1:
                                await PrintAddGameAsync();
                                break;
                            case 2:
                                await PrintDeleteGameAsync();
                                break;
                            case 3:
                                await PrintModifyGameAsync();
                                break;
                            case 4:
                                await PrintQualifyGameAsync();
                                break;
                            case 5:
                                await PrintViewGameDetailsAsync();
                                break;
                            case 6:
                                await PrintViewGamesAsync();
                                break;
                            case 7:
                                await PrintSearchForGameAsync();
                                break;
                            case 8:
                                await PrintSeeMyGamesAsync();
                                break;
                            case 9:
                                await PrintBuyGameAsync();
                                break;
                            case 10:
                                await PrintDownloadGameCover();
                                break;
                            case 0:
                                PrintLogout();
                                break;
                            default:
                                break;
                        }
                    }
                    catch (IOException ioex)
                    {
                        Console.WriteLine(("El servidor se ha desconectado, comuníqueselo al administrador \n e intente " +
                                           "nuevamente más tarde."));
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Hubo un problema: {0}", ex.Message);
                        Console.ReadLine();
                    }
                    finally
                    {
                        Console.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Hubo un problema: {0}", ex.Message);
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine();
            }
        }

        static int PrintMenu()
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Elija una de las siguientes opciones: ");
            Console.WriteLine("1 - Agregar juego");
            Console.WriteLine("2 - Borrar juego");
            Console.WriteLine("3 - Modificar juego");
            Console.WriteLine("4 - Calificar juego");
            Console.WriteLine("5 - Ver detalle de juego");
            Console.WriteLine("6 - Ver catalogo de juegos");
            Console.WriteLine("7 - Buscar juego");
            Console.WriteLine("8 - Ver juegos comprados");
            Console.WriteLine("9 - Comprar juego");
            Console.WriteLine("10 - Descargar carátula de juego");
            Console.WriteLine("0 - Salir");

            try
            {
                return Int32.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Por favor seleccione una de las opciones ofrecidas.");
                return -1;
            }
        }

        static int PrintLogin()
        {
            Console.WriteLine("Bienvenido al Sistema Client");
            Console.WriteLine("Elija una de las siguientes opciones: ");
            Console.WriteLine("1 - Iniciar sesion");
            Console.WriteLine("0 - Salir");
            try
            {
                return Int32.Parse(Console.ReadLine());
            }
            catch (Exception)
            {
                Console.WriteLine("Por favor seleccione una de las opciones ofrecidas.");
                return -1;
            }
        }

        static async Task<bool> DoLogin()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string pass = Console.ReadLine();

            return await clientHandler.LoginAsync(username, pass);
        }

        private static void ObtainConfiguration()
        {
            var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true);
            _configuration = builder.Build();
        }

        private static async Task PrintSeeMyGamesAsync()
        {
            await clientHandler.ViewBoughtGamesAsync();
        }

        private static async Task PrintBuyGameAsync()
        {
            try
            {
                Console.Write("Comprar el juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                await clientHandler.BuyGameAsync(id);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }

        private static void PrintLogout()
        {
            Console.WriteLine("Cerrando conexion con el servidor");
            clientHandler.Logout();
            connected = false;
            Console.WriteLine("Conexion cerrada con exito");
        }

        private static async Task PrintViewGamesAsync()
        {
            Console.WriteLine("Juegos en el sistema: ");
            await clientHandler.ViewGamesAsync();
        }

        private static async Task PrintViewGameDetailsAsync()
        {
            try
            {
                Console.Write("Ver detalle del juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                await clientHandler.ViewGameDetailAsync(id);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }

        private static async Task PrintQualifyGameAsync()
        {
            try
            {
                Console.Write("Calificar juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                Console.Write("Puntaje: ");
                int rating = Int32.Parse(Console.ReadLine());

                Console.Write("Reseña: ");
                string content = Console.ReadLine();

                await clientHandler.QualifyGameAsync(id, rating, content);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }

        private static async Task PrintModifyGameAsync()
        {
            try
            {
                Console.Write("Modificar juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                Console.Write("Nombre: ");
                var title = Console.ReadLine();

                Console.Write("Genero: ");
                var genre = Console.ReadLine();

                Console.Write("Descripcion: ");
                var trailer = Console.ReadLine();

                Console.Write("Ruta a la imagen: ");
                var cover = Console.ReadLine();

                await clientHandler.ModifyGameAsync(id, title, genre, trailer, cover);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }

        private static async Task PrintDeleteGameAsync()
        {
            try
            {
                Console.Write("Borrar juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                await clientHandler.DeleteGameAsync(id);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }

        private static async Task PrintAddGameAsync()
        {
            Console.WriteLine("Agregar juego");
            Console.Write("Nombre: ");
            var title = Console.ReadLine();

            Console.Write("Genero: ");
            var genre = Console.ReadLine();

            Console.Write("Descripcion: ");
            var trailer = Console.ReadLine();

            Console.Write("Ruta a la imagen: ");
            var cover = Console.ReadLine();

            await clientHandler.AddGameAsync(title, genre, trailer, cover);
        }

        private static async Task PrintSearchForGameAsync()
        {
            string searchMode = "", searchTerm = "", minRating = "";

            Console.WriteLine("Buscar por:");
            Console.WriteLine("1 - Nombre");
            Console.WriteLine("2 - Genero");
            Console.WriteLine("3 - Puntaje minimo");
            Console.Write("Modo: ");

            searchMode = Console.ReadLine();

            if (searchMode == "1" || searchMode == "2")
            {
                Console.Write("Filtro: ");
                searchTerm = Console.ReadLine();
            }
            else
            {
                Console.Write("Puntaje minimo: ");
                minRating = Console.ReadLine();
            }


            await clientHandler.SearchForGamesAsync(searchMode, searchTerm, minRating);
        }

        private static async Task PrintDownloadGameCover()
        {
            try
            {
                Console.Write("Ver carátula de juego con el id: ");
                int id = Int32.Parse(Console.ReadLine());

                await clientHandler.DownloadGameCoverAsync(id);
            }
            catch (Exception e)
            {
                throw new Exception("Por favor envíe los datos en su correspondiente tipo.");
            }
        }
    }
}