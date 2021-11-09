using DTOs;
using StateServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateServer.Repositories
{
    public class ReviewRepository : IRepository<ReviewDTO>
    {

        private readonly IDictionary<int, ReviewDTO> Reviews;
        private int NextId;
        private static ReviewRepository Instance;

        private static readonly object ReviewsLocker = new object();

        public ReviewRepository()
        {
            NextId = 1;
            Reviews = new Dictionary<int, ReviewDTO>();
        }

        public ReviewRepository GetInstance()
        {
            if (Instance == null)
            {
                Instance = new ReviewRepository();
            }

            return Instance;
        }

        public void QualifyGame(int gameId, int rating, string review)
        {
            ReviewDTO aux = new ReviewDTO { GameId = gameId, Rating = rating, Content = review };
            Add(aux);
        }

        public int GetGameRating(int gameId)
        {
            List<int> ratings = new List<int>();
            foreach(ReviewDTO review in GetAll())
            {
                if (review.GameId == gameId)
                {
                    ratings.Add(review.Rating);
                }
            }

            return (int)ratings.Average();
        }

        public void Add(ReviewDTO entity)
        {
            lock (ReviewsLocker)
            {
                entity.Id = NextId++;
                Reviews.Add(entity.Id, entity);
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ReviewDTO Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("No hay review asociada al id recibido");
            lock (ReviewsLocker)
            {
                return Reviews[id];
            }
        }

        public IQueryable<ReviewDTO> GetAll()
        {
            lock (ReviewsLocker)
            {
                return (IQueryable<ReviewDTO>)Reviews.Values;
            }
        }

        public void Update(int id, ReviewDTO newEntity)
        {
            throw new NotImplementedException();
        }

        private bool ValidId(int id)
        {
            return id <= GetAll().ToList().Count;
        }

        IRepository<ReviewDTO> IRepository<ReviewDTO>.GetInstance()
        {
            throw new NotImplementedException();
        }
    }
}
