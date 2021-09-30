using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateServices
{
    public class GameRepository : IRepository<Game>
    {

        public GameRepository() { }

        public void Add(Game entity)
        {
            if(!ValidName(entity.Name))
                throw new Exception("Ya existe un juego ese nombre, por favor seleccione otro.");
            var auxList = ServerState.GetInstance().Games;
            entity.isDeleted = false;
            entity.Id = ObtainId();
            auxList.Add(entity);
            ServerState.GetInstance().Games = auxList;
        }

        private bool ValidName(string name)
        {
            var auxList = ServerState.GetInstance().Games;
            Game game = auxList.Find(x => x.Name.Equals(name));
            return game == null || game.isDeleted;
        }

        public int ObtainId()
        {
            var auxList = ServerState.GetInstance().Games;
            var id = auxList.Count() + 1;
            return id;
        }

        public void Delete(int id)
        {
            if (!ValidId(id))
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            Game game = Get(id);
            game.isDeleted = true;
        }

        public void QualifyGame(int id, int rating, string review)
        {
            if (!ValidId(id))
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            Game game = Get(id);
            Review newReview = CreateReview(id, rating, review);
            var auxList = ServerState.GetInstance().Games;
            auxList.Find(x => x.Id == id).Reviews.Add(newReview);
            auxList.Find(x => x.Id == id).Rating = GetRating(id);
        }

        private int GetRating(int gameId)
        {
            List<int> ratings = new List<int>();

            foreach (var review in Get(gameId).Reviews)
            {
                ratings.Add(review.Rating);
            }

            return (int)ratings.Average();
        }

        public Review CreateReview(int gameId, int rating, string review)
        {
            if (!ValidId(gameId))
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            Game game = Get(gameId);
            int reviewId = game.Reviews.Count() + 1;
            Review newReview = new Review{ Id = reviewId, Rating = rating, Content =review};
            return newReview;
        }

        public Game Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            Game game = ServerState.GetInstance().Games.Find(x => x.Id == id);
            if (game.isDeleted)
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            return ServerState.GetInstance().Games.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= ServerState.GetInstance().Games.Count();
        }

        public IQueryable<Game> GetAll()
        {
            return ServerState.GetInstance().Games.ToList().AsQueryable();
        }

        public void Update(int id, Game newEntity)
        {
            if (!ValidId(id))
                throw new Exception("No existe un juego relacionado al id proporcionado.");
            //if(!ValidName(newEntity.Name))
            //    throw new Exception("Ya existe un juego ese nombre, por favor seleccione otro.");
            //tuve q comentar pq no deja actualizar el juego con su mismo nombre y quiero debuggear
            Game old = Get(id);
            old.Name = newEntity.Name;
            old.Genre = newEntity.Genre;
            old.CoverPath = newEntity.CoverPath;
            old.Description = newEntity.Description;
        }

    }

}
