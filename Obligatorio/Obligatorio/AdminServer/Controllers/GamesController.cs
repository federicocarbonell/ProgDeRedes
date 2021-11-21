using DTOs;
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
        private Reviews.ReviewsClient reviewClient;

        public GamesController()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            client = new Game.GameClient(channel);
            reviewClient = new Reviews.ReviewsClient(channel);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();

            var reply = await client.GetAllGamesAsync(request).ResponseAsync;
            
            return Ok(reply.Games);
        }

        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] GameDTO game)
        {
            var request = new GameMessage { Id = 0, Name = game.Name, CoverPath = "undefined", Description = game.Description, Genre = game.Genre, IsDeleted = false };
            var reply = await client.AddGameAsync(request);
            return Ok(reply.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            GameDetail response = new GameDetail();
            GameId gameReq = new GameId { Id = (int)id };
            GameIdMessage reviewReq = new GameIdMessage { Id = (int)id };

            var gameInfo = await client.GetGameDetailAsync(gameReq);
            var gameReviews = await reviewClient.GetReviewsByGameIdAsync(reviewReq);

            response.GameInfo = $"id = {gameInfo.Id}, name = {gameInfo.Name}, genre = {gameInfo.Genre}, description = {gameInfo.Description}";
            foreach(var review in gameReviews.Reviews)
            {
                response.GameReviews.Add(ConvertToDTO(review));
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var request = new GameId { Id = id };
            await client.DeleteGameAsync(request);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromBody] GameDTO game, int id)
        {
            var request = new GameMessage { Id = id, Name = game.Name, CoverPath = "undefined", Description = game.Description, Genre = game.Genre, IsDeleted = game.IsDeleted };
            var aux = new ModifyGameRequest { GameId = id, Game = request };
            var response = await client.ModifyGameAsync(aux);

            return NoContent();
        }

        [HttpPost("{id}/reviews")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO review)
        {
            ReviewMessage message = new ReviewMessage { Id = 0, GameId = review.GameId, Content = review.Content, Rating = review.Rating };
            AddReviewResponse response = await reviewClient.AddReviewAsync(message);

            return Ok(response);
        }

        [HttpPost("{id}/owners")]
        public async Task<IActionResult> BuyGame([FromQuery] string buyerName, int id)
        {
            BuyGameRequest message = new BuyGameRequest{ GameId = id, Owner = buyerName };
            GenResponse response = await client.BuyGameAsync(message);
            if(response.Ok) Ok(response.Message);
            return BadRequest(response.Message);
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

        private ReviewDTO ConvertToDTO(ReviewMessage review)
        {
            return new ReviewDTO { Id = review.Id, GameId = review.GameId, Content = review.Content, Rating = review.Rating };
        }

        private struct GameDetail
        {
            public string GameInfo;
            public List<ReviewDTO> GameReviews;
        }

    }
}
