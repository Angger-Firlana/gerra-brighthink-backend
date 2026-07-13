using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Data;
using backend.Models;
using backend.DTOs.Api;
using backend.DTOs.User;
using Microsoft.EntityFrameworkCore;
using backend.Wrapper;

namespace backend.Features.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext dbContext;
         public UserService(AppDbContext context)
        {
            dbContext = context;
        }

        public async Task<PagedResult<Models.User>> GetUsers(int page, int pageSize,string? search = null)
        {
            var query = dbContext.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u=> u.Username.Contains(search) || u.Name.Contains(search));
            }
            
            var totalItems = await query.CountAsync();

            var users = await query.Include(u=> u.Role)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            return new PagedResult<Models.User>
            {
                Items = users,
                pagination = new Pagination
                {
                    page = page,
                    pageSize = pageSize,
                    total = totalItems,
                    totalPages =(int) Math.Ceiling((double)totalItems / pageSize),
                    hasNextPage = page * pageSize < totalItems,
                    hasPreviousPage = page > 1
                }
            };
        }
        public async Task<Models.User> Create(CreateUserRequest request)
        {
            var user = new Models.User();
            if(string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Username, Name, Email, and Password cannot be null or empty.");
            }
            var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.Username = request.Username;
            user.Name = request.Name;
            user.Email = request.Email;
            user.Password = password;
            user.IsActive = request.IsActive;
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
                    var password = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    user.Password = password;
                }
                if (request.IsActive.HasValue)
                {
                    user.IsActive = request.IsActive.Value;
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

                user.DeletedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
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