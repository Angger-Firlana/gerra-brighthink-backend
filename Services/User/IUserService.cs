using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.User;
using backend.wrapper;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace backend.Services.User
{
    public interface IUserService
    {
        Task<PagedResult<Models.User>> GetUsers(int page, int pageSize, string? search);
        Task<Models.User> Create(CreateUserRequest request);

        Task<Models.User?> GetById(int id);

        Task<Models.User> Update(
            int id,
            UpdateUserRequest request
        );

        Task<bool> SoftDelete(int id);

        Task<bool> Delete(int id);
    }
}