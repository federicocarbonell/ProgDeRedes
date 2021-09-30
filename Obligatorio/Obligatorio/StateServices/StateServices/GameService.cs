using System;
using System.Collections.Generic;
using System.Linq;
using DTOs;
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
            Game gameToInsert = new Game
            {
                Name = game.Name,
                Genre = game.Genre,
                CoverPath = game.CoverPath,
                Description = game.Description,
                Reviews = new List<Review>(),
            };
            try
            {
                gameRepository.Add(gameToInsert);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetAllGames()
        {
            string games = "";
            foreach (Game game in gameRepository.GetAll())
            {
                if (!game.isDeleted)
                {
                    games += $"Id: {game.Id} Nombre: {game.Name} \n";
                }
            }
            return games;
        }

        public string GetAllBoughtGames(string username)
        {
            string games = "";

            foreach (Game game in gameRepository.GetAll())
            {
                if (game.owners != null && game.owners.Contains(username))
                {
                    games += $"Id: {game.Id} Nombre: {game.Name} \n";
                }
            }

            if (string.IsNullOrEmpty(games))
                games = "El usuario especificado no ha adquirido juegos";

            return games;
        }

        public void BuyGame(Tuple<int, string> data)
        {
            Game gameCopy = gameRepository.Get(data.Item1);

            if (gameCopy.owners == null)
            {
                gameCopy.owners = new List<string>();
            }

            gameCopy.owners.Add(data.Item2);

            gameRepository.Update(gameCopy.Id, gameCopy);

        }

        public string GetAllByQuery(Tuple<int, string, int> queryData)
        {
            IQueryable<Game> games = gameRepository.GetAll();
            string result = "";
            int queryType = queryData.Item1;

            switch (queryType)
            {
                case 1:
                    {
                        foreach (Game game in games.Where(x => x.Name.Contains(queryData.Item2) && !x.isDeleted))
                        {
                            result += $"Id: {game.Id} Nombre: {game.Name} \n";
                        }
                    }
                    break;
                case 2:
                    {
                        foreach (Game game in games.Where(x => x.Genre.Equals(queryData.Item2) && !x.isDeleted))
                        {
                            result += $"Id: {game.Id} Nombre: {game.Name} \n";
                        }
                    }
                    break;
                case 3:
                    {
                        foreach (Game game in games.Where(x => x.Rating >= queryData.Item3 && !x.isDeleted))
                        {
                            result += $"Id: {game.Id} Nombre: {game.Name} \n";
                        }
                    }
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(result)) result = "No se encontraron juegos con el filtro especificado";

            return result;
        }

        public string GetGameDetail(int gameId)
        {
            string details = "";
            try
            {
                Game game = gameRepository.Get(gameId);
                details += $"Id: {game.Id}, Nombre: {game.Name} \n";
                details += $"Categoria: {game.Genre} , Descripcion: {game.Description} \n";
                double rating = 0;
                details +=  "Reviews: \n";
                foreach (var review in game.Reviews)
                {
                    details += $"Id: {review.Id}, Rating: {review.Rating}" +
                        $", Reseña: {review.Content}, \n";
                    rating += review.Rating;
                }
                details += $"Rating promedio: {rating / game.Reviews.Count} \n";
                return details;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void DeleteGame(int id)
        {
            try
            {
                gameRepository.Delete(id);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
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
            try
            {
                gameRepository.Update(id, gameToModify);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public void QualifyGame (ReviewDTO reviewDTO)
        {
            try
            {
                gameRepository.QualifyGame(reviewDTO.GameId, reviewDTO.Rating, reviewDTO.Content);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
