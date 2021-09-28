﻿using System;
namespace Client
{
    public class ClientProgram
    {
        static ClientHandler clientHandler;


        static void Main(string[] args)
        {
            clientHandler = new ClientHandler();
            int command = -1;
            do
            {
                try
                {
                    command = PrintMenu();
                    switch (command)
                    {
                        case 1:
                            PrintAddGame();
                            break;
                        case 2:
                            PrintDeleteGame();
                            break;
                        case 3:
                            PrintModifyGame();
                            break;
                        case 4:
                            PrintQualifyGame();
                            break;
                        case 5:
                            PrintViewGameDetails();
                            break;
                        case 6:
                            PrintViewGames();
                            break;
                        case 7:
                            PrintSearchForGame();
                            break;
                        case 0:
                            PrintLogout();
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Excepcion inesperada: {0}", ex.Message);
                }
                finally
                {
                    Console.WriteLine();
                }
            } while (command != 0);
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

        private static void PrintLogout()
        {
            Console.WriteLine("Cerrando conexion con el servidor");
            clientHandler.Logout();
            Console.WriteLine("Conexion cerrada con exito");
        }

        private static void PrintViewGames()
        {
            Console.WriteLine("Juegos en el sistema: ");
            clientHandler.ViewGames();
        }

        private static void PrintViewGameDetails()
        {
            Console.Write("Ver detalle del juego con el id: ");
            int id = Int32.Parse(Console.ReadLine());

            clientHandler.ViewGameDetail(id);
        }

        private static void PrintQualifyGame()
        {
            Console.Write("Calificar juego con el id: ");
            int id = Int32.Parse(Console.ReadLine());

            Console.Write("Puntaje: ");
            int rating = Int32.Parse(Console.ReadLine());

            Console.Write("Reseña: ");
            string content = Console.ReadLine();

            clientHandler.QualifyGame(id, rating, content);

        }

        private static void PrintModifyGame()
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

            clientHandler.ModifyGame(id, title, genre, trailer, cover);
        }

        private static void PrintDeleteGame()
        {
            Console.Write("Borrar juego con el id: ");
            int id = Int32.Parse(Console.ReadLine());

            clientHandler.DeleteGame(id);
        }

        private static void PrintAddGame()
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

            clientHandler.AddGame(title, genre, trailer, cover);
        }

        private static void PrintSearchForGame()
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


            clientHandler.SearchForGames(searchMode, searchTerm, minRating);
        }

    }
}
