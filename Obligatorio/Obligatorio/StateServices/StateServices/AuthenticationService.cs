using DTOs;

using StateServices.DomainEntities;

using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;

namespace StateServices
{
    public class AuthenticationService
    {

        private UserRepository userRepository;
        private User loggedUser;

        public AuthenticationService(UserRepository repository)
        {
            userRepository = repository;
        }

        public User GetLoggedUser()
        {
            return loggedUser;
        }

        public List<User> GetUsers()
        {
            return userRepository.GetAll().ToList();
        }

        public User AddUser(string username, string password)
        {
            User newUser = new User() { Username = username, Password = password };
            if (!ValidUsername(username))
                throw new Exception($"Error agregando usuario, el nombre {username} no esta disponible.");

            newUser.Id = userRepository.GetAll().Count() + 1;
            newUser.OwnedGames = new List<Game>();

            userRepository.Add(newUser);
            
            return newUser;
        }

        public bool Login(string username, string password)
        {
            User aux = GetByName(username);
            if (aux == null)
                return false;
            if (aux.Password.Equals(password))
            {
                loggedUser = aux;
                return true;
            }
            return false;
        }

        public void DeleteUser(int userId)
        {
            User user = userRepository.Get(userId);
            user.IsDeleted = true;
            userRepository.Update(userId, user);
        }

        public void UpdateUser(int userId, string username, string password)
        {
            User user = userRepository.Get(userId);
            user.Username = username;
            user.Password = password;
            userRepository.Update(userId, user);
        }

        private bool ValidUsername(string username)
        {
            List<string> usernameList = userRepository.GetAll().Select(x => x.Username).ToList();

            return !usernameList.Contains(username);
        }

        private User GetByName(string name)
        {
            return userRepository.GetAll().Where(x => x.Username.Equals(name)).FirstOrDefault();
        }

        private User FromDto(UserDTO dto)
        {
            return new User { Username = dto.Username, Password = dto.Password };
        }
    }
}
