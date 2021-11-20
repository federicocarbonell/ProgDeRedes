using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StateServer.Interfaces;

namespace StateServer.Services
{
    public class UserService : User.UserBase
    {
        private readonly ILogger<UserService> _logger;
        private readonly IRepository<UserDTO> UserRepository;
        public UserService(ILogger<UserService> logger, IRepository<UserDTO> userRepo)
        {
            _logger = logger;
            UserRepository = userRepo;
        }

        public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            UserDTO aux = GetByName(request.Username);
            if (aux == null)
                return Task.FromResult(new LoginResponse
                {
                    Response = false,
                    LoggedUser = ""
                });
            if (aux.Password.Equals(request.Password))
            {
                return Task.FromResult(new LoginResponse
                {
                    Response = true,
                    LoggedUser = aux.Username
                });
            }
            return Task.FromResult(new LoginResponse
            {
                Response = false,
                LoggedUser = ""
            });
        }

        public Task<GenericResponse> AddUser(UserMessage message)
        {
            try
            {
                UserRepository.Add(FromMessage(message));
                string returnMessage = $"Usuario {message.Username} dado de alta con exito";
                return Task.FromResult(new GenericResponse { Ok = true, Messsage = returnMessage });
            }
            catch (Exception e)
            {
                return Task.FromResult(new GenericResponse { Ok = false, Messsage = e.Message });
            }
        }

        public Task<GenericResponse> DeleteUser(UserIdMessage message)
        {
            try
            {
                UserRepository.Delete(message.Id);
                string returnMessage = $"Usuario con el id {message.Id} borrado con exito";
                return Task.FromResult(new GenericResponse { Ok = true, Messsage = returnMessage });
            }
            catch (Exception e)
            {
                return Task.FromResult(new GenericResponse { Ok = false, Messsage = e.Message });
            }
        }

        public Task<UserListMessage> GetUsers()
        {
            var returnMessage = new UserListMessage();
            foreach (UserDTO item in UserRepository.GetAll())
            {
                returnMessage.Users.Add(FromDTO(item));
            }

            return Task.FromResult(returnMessage);
        }

        public Task<GenericResponse> ModifyUser(UserMessage message)
        {
            try
            {
                UserDTO newUser = FromMessage(message);
                UserRepository.Update(newUser.Id, newUser);
                string returnMessage = $"Usuario con el id {message.Id} actualizado con exito";
                return Task.FromResult(new GenericResponse { Ok = true, Messsage = returnMessage });
            }
            catch (Exception e)
            {
                return Task.FromResult(new GenericResponse { Ok = false, Messsage = e.Message });
            }
            
        }

        private UserDTO GetByName(string name)
        {
            return UserRepository.GetAll().Where(x => x.Username.Equals(name)).FirstOrDefault();
        }

        private UserDTO FromMessage(UserMessage userMessage)
        {
            return new UserDTO 
            { 
                Id = userMessage.Id,
                IsDeleted = userMessage.IsDeleted,
                Username = userMessage.Username,
                Password = userMessage.Password 
            };
        }

        private UserMessage FromDTO(UserDTO user)
        {
            return new UserMessage
            {
                Id = user.Id,
                IsDeleted = user.IsDeleted,
                Username = user.Username
            };
        }
    }
}
