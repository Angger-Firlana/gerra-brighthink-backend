using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using backend.Data;
using backend.DTOs.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BCrypt.Net;
using backend.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace backend.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext dbContext;
        private readonly GenerateToken generateToken;
        public AuthService(AppDbContext dbContext, GenerateToken generateToken)
        {
            this.dbContext = dbContext;
            this.generateToken = generateToken;
        }
        public async Task<LoginResponse> Login(string identity, string password)
        {
            var response = new LoginResponse();

            var user = dbContext.Users.Include(u=> u.Role).FirstOrDefault(x=> x.Email == identity || x.Username == identity);
            var isValidPassword = BCrypt.Net.BCrypt.Verify(
                password,
                user!.Password
            );
            if (user == null || !isValidPassword)
            {
                response.code = 404;
                response.message = "username or password is wrong";
                response.token = null;
                response.user = null;
                
            }

            var token = generateToken.execute(user!);

            response.code = 200;
            response.message = "login successfully";
            response.token = token;
            response.user = user;

            return response;
        }

        public async Task<Models.User> GetMe()
        {
            return new Models.User();
        }
    }
}