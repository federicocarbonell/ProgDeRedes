using System;
using System.Threading.Tasks;
using DTOs;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StateServer.Interfaces;

namespace StateServer
{
    public class GameService : Game.GameBase
    {
        private readonly ILogger<GameService> _logger;
        private readonly IRepository<GameDTO> GameRepository;

        public GameService(ILogger<GameService> logger, IRepository<GameDTO> gameRepo)
        {
            _logger = logger;
            GameRepository = gameRepo;
        }

        public override Task<AddGameReply> AddGame(GameMessage request, ServerCallContext context)
        {
            try
            {
                GameRepository.GetInstance().Add(FromMessage(request));
                return Task.FromResult(new AddGameReply
                {
                    Message = $"Added game {FromMessage(request).Name} successfully"
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new AddGameReply
                {
                    Message = e.Message
                });
            }
        }

        public override Task<GamesList> GetAllGames(Empty request, ServerCallContext context)
        {
            try
            {
                var games = GameRepository.GetInstance().GetAll();
                var gamesList = new GamesList();

                foreach(var game in GameRepository.GetInstance().GetAll())
                {
                    gamesList.Games.Add(new GameMessage { Id = game.Id, Name = game.Name, Genre = game.Genre, Description = game.Description, CoverPath = game.CoverPath });
                }

                return Task.FromResult(gamesList);
            }
            catch (Exception e)
            {
                return Task.FromResult(new GamesList());
            }
        }

        private GameDTO FromMessage(GameMessage message)
        {
            return new GameDTO
            {
                Id = message.Id,
                Description = message.Description,
                Name = message.Name,
                Genre = message.Genre,
                CoverPath = message.CoverPath,
                IsDeleted = message.IsDeleted
            };
        }
    }
}
