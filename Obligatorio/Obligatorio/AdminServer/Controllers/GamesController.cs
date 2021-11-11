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
            
            return Ok(ConvertToString(reply));
        }

        [HttpPost]
        public async Task<IActionResult> AddGame([FromBody] GameDTO game)
        {
            var request = new GameMessage { Id = 0, Name = game.Name, CoverPath = game.CoverPath, Description = game.Description, Genre = game.Genre, IsDeleted = false };
            var reply = await client.AddGameAsync(request);
            return Ok(reply.Message);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            GameDetail response = new GameDetail();
            GameId gameReq = new GameId { GameId_ = (int)id };
            GameIdMessage reviewReq = new GameIdMessage { Id = (int)id };

            var gameInfo = await client.GetGameDetailAsync(gameReq);
            var gameReviews = await reviewClient.GetReviewsByGameIdAsync(reviewReq);

            response.GameInfo = gameInfo.Details;
            foreach(var review in gameReviews.Reviews)
            {
                response.GameReviews.Add(ConvertToDTO(review));
            }

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var request = new GameId { GameId_ = (int)id };
            await client.DeleteGameAsync(request);

            return NoContent();
        }

        [HttpPost("{id}/reviews")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO review)
        {
            ReviewMessage message = new ReviewMessage { Id = 0, GameId = review.GameId, Content = review.Content, Rating = review.Rating };
            AddReviewResponse response = await reviewClient.AddReviewAsync(message);

            return Ok(response);
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
