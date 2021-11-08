using System;
using System.Threading.Tasks;
using DTOs;
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

        public async Task AddGame(GameMessage gameMessage)
        {
            try
            {
                GameRepository.GetInstance().Add(FromMessage(gameMessage));
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
