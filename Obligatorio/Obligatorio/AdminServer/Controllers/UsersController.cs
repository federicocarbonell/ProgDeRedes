using System;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

using AdminServer.Protos;
using DTOs;

namespace AdminServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {

        private GrpcChannel channel;
        
        private User.UserClient client;

        private Game.GameClient gameClient;

        public UsersController()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            client = new User.UserClient(channel);
            gameClient = new Game.GameClient(channel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();

            var reply = await client.GetUsersAsync(request).ResponseAsync;

            return Ok(reply.Users);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody]UserDTO user)
        {
            var request = new UserMessage
            {
                Username = user.Username,
                Password = user.Password
            };

            var reply = await client.AddUserAsync(request).ResponseAsync;

            if(reply.Ok) return Ok();
            
            return BadRequest(reply.Messsage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = new UserIdMessage{ Id = id };
            var response = await client.DeleteUserAsync(request);

            if(response.Ok) return NoContent();
            return BadRequest(response.Messsage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromBody]UserDTO user, int id)
        {
            var request = new UserMessage
            {
                Id = id,
                Username = user.Username,
                Password = user.Password
                
            };

            var reply = await client.ModifyUserAsync(request).ResponseAsync;

            if(reply.Ok) return Ok();
            
            return BadRequest(reply.Messsage);
        }

        [HttpPost("{buyerId}/games")]
        public async Task<IActionResult> BuyGame([FromQuery] int gameId, int buyerId)
        {
            var getUsernameMessage = new UserIdMessage { Id = buyerId };
            GenericResponse usernameResponse = await client.GetUsernameAsync(getUsernameMessage);
            
            BuyGameRequest message = new BuyGameRequest 
            { 
                GameId = gameId,
                Owner = usernameResponse.Messsage
            };
            GenResponse response = await gameClient.BuyGameAsync(message);
            
            if (response.Ok) Ok(response.Message);
            return BadRequest(response.Message);
        }
    }
}