using System;
using System.Linq;
using Client.DTOs;
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

        public void AddGame(GameDTO game)
        {
            // TODO: Mapper
            Game gameToInsert = new Game
            {
                Name = game.Name,
                Genre = game.Genre,
                CoverPath = game.CoverPath,
                Description = game.Description
            };
            gameRepository.Add(gameToInsert);
        }

        public IQueryable<Game> GetAllGames()
        {
            GameRepository gameRepository = new GameRepository();
            return gameRepository.GetAll();
        }

        public void DeleteGame(int id)
        {
            gameRepository.Delete(id);
        }

        public void ModifyGame(int id, GameDTO game)
        {
            Game gameToModify = new Game
            {
                Id = game.Id,
                Name = game.Name,
                Genre = game.Genre,
                CoverPath = game.CoverPath,
                Description = game.Description
            };
            gameRepository.Update(id, gameToModify);
        }

        public void QualifyGame (ReviewDTO reviewDTO)
        {
            gameRepository.QualifyGame(reviewDTO.GameId, reviewDTO.Rating, reviewDTO.Content);
        }
    }
}
