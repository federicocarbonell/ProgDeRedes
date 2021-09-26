using System;
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
                    Console.WriteLine("Unexpected exception: {0}", ex.Message);
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
            Console.WriteLine("Opciones validas: ");
            Console.WriteLine("1- Add game");
            Console.WriteLine("2- Delete game");
            Console.WriteLine("3- Modify game");
            Console.WriteLine("4- Qualify game");
            Console.WriteLine("5- View game detail");
            Console.WriteLine("6- View all games");
            Console.WriteLine("7- Search for game");
            Console.WriteLine("0- Exit");
            Console.WriteLine("Choose menu number: ");
            try
            {
                return Int32.Parse(Console.ReadLine());

            }
            catch (Exception)
            {
                Console.WriteLine("Invalid input.");
                return -1;
            }
        }

        private static void PrintLogout()
        {
            throw new NotImplementedException();
        }

        private static void PrintViewGames()
        {
            Console.WriteLine("Games: ");
            clientHandler.ViewGames();
        }

        private static void PrintViewGameDetails()
        {
            Console.WriteLine("View game details");
            Console.Write("Game id: ");
            int id = Int32.Parse(Console.ReadLine());

            clientHandler.ViewGameDetail(id);
        }

        private static void PrintQualifyGame()
        {
            Console.WriteLine("Qualify game");
            Console.Write("Game id: ");
            int id = Int32.Parse(Console.ReadLine());

            Console.Write("Game rating: ");
            int rating = Int32.Parse(Console.ReadLine());

            Console.Write("Game content: ");
            string content = Console.ReadLine();

            Console.Write("--- Game qualified ---");
            clientHandler.QualifyGame(id, rating, content);

        }

        private static void PrintModifyGame()
        {
            Console.WriteLine("Modify game");
            Console.Write("Game id: ");
            int id = Int32.Parse(Console.ReadLine());

            Console.Write("Game title: ");
            var title = Console.ReadLine();

            Console.Write("Game genre: ");
            var genre = Console.ReadLine();

            Console.Write("Game trailer: ");
            var trailer = Console.ReadLine();

            Console.Write("Game cover: ");
            var cover = Console.ReadLine();

            clientHandler.ModifyGame(id, title, genre, trailer, cover);
            Console.Write("--- Game modified ---");
        }

        private static void PrintDeleteGame()
        {
            Console.WriteLine("Delete game");
            Console.Write("Game id: ");
            int id = Int32.Parse(Console.ReadLine());

            clientHandler.DeleteGame(id);
            Console.Write("--- Game deleted ---");
        }

        private static void PrintAddGame()
        {
            Console.WriteLine("Add game");
            Console.Write("Game title: ");
            var title = Console.ReadLine();

            Console.Write("Game genre: ");
            var genre = Console.ReadLine();

            Console.Write("Game sinopsis: ");
            var trailer = Console.ReadLine();

            Console.Write("Game cover: ");
            var cover = Console.ReadLine();

            clientHandler.AddGame(title, genre, trailer, cover);
            Console.Write("--- Game added ---");
        }

        private static void PrintSearchForGame()
        {
            string searchMode = "", searchTerm = "", minRating = "";
            
            Console.WriteLine("Search for name by :");
            Console.WriteLine("1: Title");
            Console.WriteLine("2: Category");
            Console.WriteLine("3: Minimum rating");
            Console.Write("Mode: ");
            
            searchMode = Console.ReadLine();

            if(searchMode == "1" || searchMode == "2")
            {
                Console.Write("Filter: ");
                searchTerm = Console.ReadLine();
            }
            else
            {
                Console.Write("Minimum rating: ");
                minRating = Console.ReadLine();
            }
            Console.Write("--- Search ongoing ---");

            clientHandler.SearchForGames(searchMode, searchTerm, minRating);
        }

    }
}
