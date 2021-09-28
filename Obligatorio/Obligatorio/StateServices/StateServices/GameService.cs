using System;
using System.Collections.Generic;
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
                Description = game.Description,
                Reviews = new List<Review>(),
            };
            gameRepository.Add(gameToInsert);
        }

        public String GetAllGames()
        {
            String games = "";
            foreach (Game game in gameRepository.GetAll())
            {
                if (!game.isDeleted)
                {
                    games += $"Id: {game.Id} Nombre: {game.Name} \n";
                }
            }
            return games;
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

        public String GetGameDetail(int gameId)
        {
            //List<string> details = new List<string>();
            //Game game = gameRepository.Get(gameId);
            //details.Add($"Id: {game.Id}, Name: {game.Name}, ");
            //details.Add($"Genre: {game.Genre} , Description: {game.Description}, ");
            //double rating = 0;
            //foreach (var review in game.Reviews)
            //{
            //    details.Add("Reviews: ");
            //    details.Add($"Id: {review.Id}, Rating: {review.Rating}" +
            //        $", Content: {review.Content}, ");
            //    rating += review.Rating;
            //}
            //details.Add($"Rating average: {rating / game.Reviews.Count}");
            //return details;

            String details = "";
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
