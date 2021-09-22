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
            var auxList = ServerState.GetInstance().Games;
            entity.isDeleted = false;
            entity.Id = obtainId();
            auxList.Add(entity);
            ServerState.GetInstance().Games = auxList;
        }//hay que hacer esta magia con las listas en todos creo

        public int obtainId()
        {
            var auxList = ServerState.GetInstance().Games;
            var id = auxList.Count() + 1;
            return id;
        }

        public void Delete(int id)
        {
            int arrPos = ServerState.GetInstance().Games.FindIndex(x => x.Id == id);
            var auxList = ServerState.GetInstance().Games;
            auxList.Find(x => x.Id == id).isDeleted = true;
            ServerState.GetInstance().Games = auxList;
        }

        public Game Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            return ServerState.GetInstance().Games.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= ServerState.GetInstance().Users.FindLast(x => x != null).Id;
        }

        public IQueryable<Game> GetAll()
        {
            return ServerState.GetInstance().Games.ToList().AsQueryable();
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
