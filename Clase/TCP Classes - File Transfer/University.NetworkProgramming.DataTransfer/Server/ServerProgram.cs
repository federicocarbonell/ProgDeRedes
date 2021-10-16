using System;

namespace Server
{
    class ServerProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server");
            var serverHandler = new ServerHandler();
            Console.WriteLine("Waiting for client and file upload");
            serverHandler.ReceiveFile();
            Console.WriteLine("Done receiving file");
        }
    }
}
