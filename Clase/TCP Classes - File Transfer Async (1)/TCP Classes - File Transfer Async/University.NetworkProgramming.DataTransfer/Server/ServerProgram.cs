using System;
using System.Threading.Tasks;

namespace Server
{
    class ServerProgram
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting server");
            var serverHandler = new ServerHandler();
            Console.WriteLine("Waiting for client and file upload");
            await serverHandler.ReceiveFileAsync();
            Console.WriteLine("Done receiving file");
        }
    }
}
