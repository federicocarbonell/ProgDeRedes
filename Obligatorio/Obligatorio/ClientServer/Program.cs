using System;
using System.Threading.Tasks;
using Grpc.Net.Client;
using StateServer;

namespace ClientServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // This switch must be set before creating the GrpcChannel/HttpClient.
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            // The port number(5000) must match the port of the gRPC server.
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client1 = new Greeter.GreeterClient(channel);
            var reply1 = await client1.SayHelloAsync(
                              new HelloRequest { Name = "GreeterClient" });
            var client = new Game.GameClient(channel);
            var reply = await client.AddGameAsync(new GameMessage { Id = 1, Name = "Test", CoverPath = "Test", Description = "Test", Genre = "Test" });
            Console.WriteLine("Greeting: " + reply);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
