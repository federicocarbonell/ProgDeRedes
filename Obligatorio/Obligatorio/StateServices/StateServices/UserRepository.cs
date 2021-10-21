using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateServices
{
    public class UserRepository : IRepository<User>
    {

        public void Add(User entity)
        {
            ServerState.GetInstance().Users.Add(entity);
        }

        public void Delete(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding user");
            ServerState.GetInstance().Users.RemoveAt(id - 1);
        }

        public User Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding user");
            return ServerState.GetInstance().Users.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= ServerState.GetInstance().Users.FindLast(x => x != null).Id;
        }

        public IQueryable<User> GetAll()
        {
            return ServerState.GetInstance().Users.ToList().AsQueryable();
        }

        public void Update(int id, User newEntity)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding user");
            User old = Get(id);
            old.Password = newEntity.Password;
            old.Username = newEntity.Password;
            old.OwnedGames = newEntity.OwnedGames;
        }
    }
}
