using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StateServices
{
    public class UserRepository : IRepository<User>
    {
        private ServerState State;

        public UserRepository(ServerState state)
        {
            State = state;
        }

        public void Add(User entity)
        {
            if (ValidUsername(entity.Username))
                State.GetInstance().Users.Add(entity);
        }

        private bool ValidUsername(string username)
        {
            List<string> usernameList = State.GetInstance().Users.Select(x => x.Username).ToList();

            return ! usernameList.Contains(username);
        }

        public void Delete(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding user");
            State.GetInstance().Users.RemoveAt(id - 1);
        }

        public User Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding user");
            return State.GetInstance().Users.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= State.GetInstance().Users.FindLast(x => x != null).Id;
        }

        public IQueryable<User> GetAll()
        {
            return State.GetInstance().Users.ToList().AsQueryable();
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
