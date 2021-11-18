using System;
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

        private UserDTO GetByName(string name)
        {
            return UserRepository.GetAll().Where(x => x.Username.Equals(name)).FirstOrDefault();
        }
    }
}
