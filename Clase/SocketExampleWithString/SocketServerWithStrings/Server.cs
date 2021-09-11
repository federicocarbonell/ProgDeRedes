using System;

namespace SocketServerWithStrings
{
    class Server
    {
        static void Main(string[] args)
        {
            SocketServer server = new SocketServer();
            server.StartListening();
        }
    }
}
