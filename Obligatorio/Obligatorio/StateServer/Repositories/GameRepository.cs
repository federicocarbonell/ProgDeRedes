using DTOs;
using StateServer.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;

namespace StateServer.Repositories
{
    public class GameRepository : IRepository<GameDTO>
    {

        private readonly IDictionary<int, GameDTO> Games;
        private int NextId;
        private static GameRepository Instance;

        private static readonly object GamesLocker = new Object();

        public GameRepository()
        {
            NextId = 1;
            Games = new Dictionary<int, GameDTO>();
        }

        public static GameRepository GetInstanceAsync()
        {
            if (Instance == null)
            {
                Instance = new GameRepository();
            }

            return Instance;
        }


        public void Add(GameDTO entity)
        {
            if (!ValidName(entity.Name))
                throw new Exception("Ya existe un juego ese nombre, por favor seleccione otro.");
            lock (GamesLocker)
            {
                entity.Id = NextId++;
                Games.Add(entity.Id, entity);
            }
        }

        public void Delete(int id)
        {
            if (!ValidId(id))
                throw new Exception("No hay un juego asociado al id recibido.");
            lock (GamesLocker)
            {
                GameDTO game = Games[id];
                game.IsDeleted = true;
            }
        }

        public GameDTO Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("No hay un juego asociado al id recibido.");
            lock (GamesLocker)
            {
                return Games[id];
            }
        }

        public IQueryable<GameDTO> GetAll()
        {
            lock (GamesLocker)
            {
                return (IQueryable<GameDTO>)Games.Values;
            }
        }

        public void Update(int id, GameDTO newEntity)
        {
            if (!ValidId(id))
                throw new Exception("No hay un juego asociado al id recibido.");
            if (!ValidName(newEntity.Name))
                throw new Exception("Ya existe un juego ese nombre, por favor seleccione otro.");
            lock (GamesLocker)
            {
                GameDTO game = Games[id];
                game.Name = newEntity.Name;
                game.Genre = newEntity.Genre;
                game.CoverPath = newEntity.CoverPath;
                game.Description = newEntity.Description;
            }
        }

        private bool ValidName(string name)
        {
            var games = GetAll().ToList();
            GameDTO g = games.Find(x => x.Name.Equals(name));
            return g == null || g.IsDeleted;
        }

        private bool ValidId(int id)
        {
            return id <= GetAll().ToList().Count;
        }

    }

}
