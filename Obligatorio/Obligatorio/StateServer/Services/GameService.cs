using System;
using System.Linq;
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

        public override Task<DeleteGameResponse> DeleteGame(GameId request, ServerCallContext context)
        {
            try
            {
                GameRepository.Delete(request.Id);
                var deleteGameResponse = new DeleteGameResponse
                {
                    Response = true
                };
                return Task.FromResult(deleteGameResponse);
            }
            catch (Exception e)
            {
                var deleteGameResponse = new DeleteGameResponse
                {
                    Response = false
                };
                return Task.FromResult(deleteGameResponse);
            }
        }

        public override Task<GameModel> GetGameDetail(GameId request, ServerCallContext context)
        {
            try
            {
                GameDTO game = GameRepository.Get(request.Id);
                var gameDetails = new GameModel { Id = game.Id, Name = game.Name, Genre = game.Genre, Description = game.Description, CoverPath = game.CoverPath };
                return Task.FromResult(gameDetails);
            }
            catch (Exception e)
            {
                return Task.FromResult(new GameModel());
            }
        }

        public override Task<ModifyGameResponse> ModifyGame(ModifyGameRequest request, ServerCallContext context)
        {
            try
            {
                GameRepository.GetInstance().Update(request.GameId, FromMessage(request.Game));
                var modifyGameResponse = new ModifyGameResponse
                {
                    Response = true
                };
                return Task.FromResult(modifyGameResponse);
            }
            catch
            {
                var modifyGameResponse = new ModifyGameResponse
                {
                    Response = false
                };
                return Task.FromResult(modifyGameResponse);
            }
        }

        public override Task<GetAllByQueryResponse> GetAllByQuery(GetAllByQueryRequest request, ServerCallContext context)
        {
            var games = GameRepository.GetAll();
            string result = "";
            int queryType = request.QueryType;

            switch (queryType)
            {
                case 1:
                    {
                        foreach (GameDTO game in games.Where(x => x.Name.Contains(request.TextQueryData) && !x.IsDeleted))
                        {
                            result += $"Id: {game.Id} Nombre: {game.Name} \n";
                        }
                    }
                    break;
                case 2:
                    {
                        foreach (GameDTO game in games.Where(x => x.Genre.Equals(request.TextQueryData) && !x.IsDeleted))
                        {
                            result += $"Id: {game.Id} Nombre: {game.Name} \n";
                        }
                    }
                    break;
                default:
                    break;
            }

            if (string.IsNullOrEmpty(result)) result = "No se encontraron juegos con el filtro especificado";

            return Task.FromResult(new GetAllByQueryResponse
            {
                Message = result
            });
        }

        public override Task<GetGameNameResponse> GetGameName(GameId request, ServerCallContext context)
        {
            try
            {
                GameDTO game = GameRepository.Get(request.Id);
                return Task.FromResult(new GetGameNameResponse
                {
                    GameName = game.Name
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new GetGameNameResponse
                {
                    GameName = ""
                });
            }
        }

        public override Task<BuyGameResponse> BuyGame(BuyGameRequest request, ServerCallContext context)
        {
            try
            {
                GameRepository.BuyGame(request.GameId, request.Owner);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
                return Task.FromResult(new BuyGameResponse
                {
                    Response = false
                });
            }
            return Task.FromResult(new BuyGameResponse
            {
                Response = true
            });
        }

        public override Task<GamesList> GetAllBoughtGames(GetAllBoughtGamesRequest request, ServerCallContext context)
        {
            try
            {
                var games = GameRepository.GetAll();
                var boughtGames = new GamesList();
                foreach (var game in games)
                {
                    if (game.Owners != null && game.Owners.Contains(request.UserName))
                    {
                        boughtGames.Games.Add(new GameMessage { Id = game.Id, Name = game.Name, Genre = game.Genre, Description = game.Description, CoverPath = game.CoverPath });
                    }
                }
                return Task.FromResult(boughtGames);
            }
            catch (Exception e)
            {
                return Task.FromResult(new GamesList());
            }
}
    }
}
