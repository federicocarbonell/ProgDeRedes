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

        public AuthenticationService(UserRepository repository)
        {
            userRepository = repository;
        }

        public User AddUser(UserDTO user)
        {
            User newUser = FromDto(user);
            
            if (!ValidUsername(user.Username))
                throw new Exception($"Error agregando usuario, el nombre {user.Username} no esta disponible.");

            newUser.Id = userRepository.GetAll().Count() + 1;
            newUser.OwnedGames = new List<Game>();

            userRepository.Add(newUser);
            
            return newUser;
        }

        public bool Login(UserDTO user)
        {
            User aux = GetByName(user.Username);
            if (aux == null)
                throw new Exception("No hay usuario con ese nombre en el sistema.");
            if (aux.Password.Equals(user.Password))
                return true;
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
