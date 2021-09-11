using System;
using System.Linq;
using StateServices.DomainEntities;

namespace StateServices
{
    public class GameService
    {
        public void AddGame(string name)
        {
            GameRepository gameRepository = new GameRepository();
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
