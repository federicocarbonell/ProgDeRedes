using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateServices
{
    public class GameRepository : IRepository<Game>
    {
        private ServerState State;

        public void Add(Game entity)
        {
            State.GetInstance().Games.Add(entity);
        }

        public void Delete(int id)
        {
            int arrPos = State.GetInstance().Games.FindIndex(x => x.Id == id);
            State.GetInstance().Games.RemoveAt(arrPos);
        }

        public Game Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            return State.GetInstance().Games.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= State.GetInstance().Users.FindLast(x => x != null).Id;
        }

        public IQueryable<Game> GetAll()
        {
            return State.GetInstance().Games.ToList().AsQueryable();
        }

        public void Update(int id, Game newEntity)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            Game old = Get(id);
            old.Genre = newEntity.Genre;
            old.Rating = newEntity.Rating;
            old.Reviews = newEntity.Reviews;
        }

    }

}
