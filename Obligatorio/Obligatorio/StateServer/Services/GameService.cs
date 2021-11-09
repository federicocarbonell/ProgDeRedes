using System;
using System.Threading.Tasks;
using DTOs;
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
                    Message = "Hello "
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

        private GameDTO FromMessage(GameMessage message)
        {
            return new GameDTO
            {
                Id = message.Id,
                Description = message.Description,
                Name = message.Name,
                Genre = message.Genre,
                CoverPath = message.CoverPath
            };
        }
    }
}
