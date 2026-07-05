using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.DTOs.User;

namespace backend.Services.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext dbContext;
         public UserService(AppDbContext context)
        {
            dbContext = context;
        }
        public async Task<Models.User> Create(CreateUserRequest request)
        {
            var user = new Models.User();
            if(string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Username, Name, Email, and Password cannot be null or empty.");
            }
            user.Username = request.Username;
            user.Name = request.Name;
            user.Email = request.Email;
            user.Password = request.Password;
            user.isActive = request.IsActive;
            user.RoleId = request.RoleId;
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return user;
        }

        public async Task<Models.User?> GetById(int id)
        {
            var user = await dbContext.Users.FindAsync(id);
            return user;
        }

        public async Task<Models.User> Update(int id, UpdateUserRequest request)
        {
            try
            {
                var user = await dbContext.Users.FindAsync(id);
                if (user == null)
                {
                    throw new ArgumentException($"User with id {id} not found.");
                }

                if (!string.IsNullOrEmpty(request.Username))
                {
                    user.Username = request.Username;
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    user.Name = request.Name;
                }
                if (!string.IsNullOrEmpty(request.Email))
                {
                    user.Email = request.Email;
                }
                if (!string.IsNullOrEmpty(request.Password))
                {
                    user.Password = request.Password;
                }
                if (request.IsActive.HasValue)
                {
                    user.isActive = request.IsActive.Value;
                }
                if (request.RoleId.HasValue)
                {
                    user.RoleId = request.RoleId.Value;
                }

                await dbContext.SaveChangesAsync();
                return user;
            }catch(Exception ex)
            {
                throw new Exception($"An error occurred while updating the user: {ex.Message}");
            }
            
        }

        public async Task<bool> SoftDelete(int id)
        {
            try
            {
                var user = await dbContext.Users.FindAsync(id);
                if (user == null)
                {
                    throw new ArgumentException($"User with id {id} not found.");
                }

                user.deleted_at = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return true;
            }catch(Exception ex)
            {
                return false;
                throw new Exception($"An error occurred while soft deleting the user: {ex.Message}");
            }
           
        }

        public async Task<bool> Delete(int id)
        {
            var user = await dbContext.Users.FindAsync(id);
            if (user == null)
            {
                throw new ArgumentException($"User with id {id} not found.");
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}