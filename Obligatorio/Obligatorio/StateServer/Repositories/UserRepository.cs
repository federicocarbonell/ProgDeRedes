using DTOs;
using StateServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateServer.Repositories
{
    public class UserRepository : IRepository<UserDTO>
    {

        private readonly IDictionary<int, UserDTO> Users;
        private int NextId;
        private static UserRepository Instance;

        private static readonly object UsersLocker = new object();

        public UserRepository()
        {
            NextId = 1;
            Users = new Dictionary<int, UserDTO>();
            Users.Add(NextId, new UserDTO()
            {
                Id = NextId,
                Username = "admin",
                Password = "admin"
            });
        }

        public static UserRepository GetInstance()
        {
            if (Instance == null)
            {
                Instance = new UserRepository();
            }

            return Instance;
        }

        public void Add(UserDTO entity)
        {
            if (!ValidName(entity.Username))
                throw new Exception("Ya existe un usuario con ese nombre, por favor seleccione otro.");
            lock (UsersLocker)
            {
                entity.Id = NextId++;
                Users.Add(entity.Id, entity);
            }
        }

        public void Delete(int id)
        {
            if (!ValidId(id))
                throw new Exception("No hay un usuario asociado al id recibido.");
            lock (UsersLocker)
            {
                UserDTO user = Users[id];
                user.IsDeleted = true;
            }
        }

        public UserDTO Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("No hay un usuario asociado al id recibido.");
            lock (UsersLocker)
            {
                return Users[id];
            }
        }

        public IQueryable<UserDTO> GetAll()
        {
            lock (UsersLocker)
            {
                return (IQueryable<UserDTO>)Users.Values;
            }
        }

        public void Update(int id, UserDTO newEntity)
        {
            if (!ValidId(id))
                throw new Exception("No hay un usuario asociado al id recibido.");
            if (!ValidName(newEntity.Username))
                throw new Exception("Ya existe un usuario con ese nombre, por favor seleccione otro.");
            lock (UsersLocker)
            {
                UserDTO user = Users[id];
                user.Username = newEntity.Username;
                user.Password = newEntity.Password;
            }
        }

        private bool ValidName(string name)
        {
            var users = GetAll().ToList();
            UserDTO u = users.Find(x => x.Username.Equals(name));
            return u == null || u.IsDeleted;
        }

        private bool ValidId(int id)
        {
            return id <= GetAll().ToList().Count && !Get(id).IsDeleted;
        }
    }
}
