using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GamesController : Controller
    {

        private GrpcChannel channel;
        private Game.GameClient client;

        public GamesController()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            client = new Game.GameClient(channel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();

            var reply = await client.GetAllGamesAsync(request).ResponseAsync;
            
            return Ok(ConvertToString(reply));
        }

        private string ConvertToString(GamesList list)
        {
            string aux = "";

            foreach (GameMessage game in list.Games)
            {
                aux += $"Id: {game.Id} Nombre: {game.Name} \n";
            }

            return aux;
        }

    }
}
