using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Auth;

namespace backend.Services.Auth
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(string identity, string password);
        Task<Models.User> GetMe();
    }
}