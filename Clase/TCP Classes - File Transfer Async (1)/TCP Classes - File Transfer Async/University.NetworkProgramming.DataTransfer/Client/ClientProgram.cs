using System;
using System.IO;
using System.Threading.Tasks;

namespace Client
{
    class ClientProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the path of the file to send");
            // Obtengo el path del archivo
            string path = Console.ReadLine();
            if (File.Exists(path))
            {
                ClientHandler clientHandler = new ClientHandler();
                Console.WriteLine("Sending file to server...");
                await clientHandler.SendFileAsync(path);
                Console.WriteLine("Done sending file");
                clientHandler.CloseConnection();
                Console.WriteLine("Client disconnected from server");
                Console.ReadLine();
            }
        }
    }
}
