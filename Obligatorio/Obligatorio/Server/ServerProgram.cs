using System;
namespace Server
{
    public class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
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
