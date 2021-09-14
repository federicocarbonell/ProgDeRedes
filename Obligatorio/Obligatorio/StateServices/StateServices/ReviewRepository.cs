using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateServices
{
    public class ReviewRepository : IRepository<Review>
    {

        public void Add(Review entity)
        {
            ServerState.GetInstance().Reviews.Add(entity);
        }

        public void Delete(int id)
        {
            int arrPos = ServerState.GetInstance().Users.FindIndex(x => x.Id == id);
            ServerState.GetInstance().Users.RemoveAt(arrPos);
        }

        public Review Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            return ServerState.GetInstance().Reviews.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= ServerState.GetInstance().Reviews.FindLast(x => x != null).Id;
        }

        public IQueryable<Review> GetAll()
        {
            return ServerState.GetInstance().Reviews.ToList().AsQueryable();
        }

        public void Update(int id, Review newEntity)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            Review old = Get(id);
            //old.Genre = newEntity.Genre;
            //old.Rating = newEntity.Rating;
            //old.Reviews = newEntity.Reviews;
        }
    }
}
