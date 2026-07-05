using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Services.Auth
{
    public interface IAuthService
    {
        Task<Models.User?> Login(string username, string password);
    }
}