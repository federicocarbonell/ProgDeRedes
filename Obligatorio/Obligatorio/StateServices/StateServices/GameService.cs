using System;
using System.Linq;
using StateServices.DomainEntities;

namespace StateServices
{
    public class GameService
    {
        GameRepository gameRepository;
        public GameService(GameRepository gameRepository)
        {
            this.gameRepository = gameRepository;
        }

        public void AddGame(string name)
        {
            Game game = new Game();
            game.Name = name;
            gameRepository.Add(game);
        }

        public IQueryable<Game> GetAllGames()
        {
            GameRepository gameRepository = new GameRepository();
            return gameRepository.GetAll();
        }
    }
}
