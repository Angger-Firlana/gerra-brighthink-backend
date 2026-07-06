using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using backend.Data;
using backend.Models;
using backend.DTOs.User;
using Microsoft.EntityFrameworkCore;
using backend.Services.User;
using backend.DTOs.Api;
using backend.wrapper;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController( IUserService userService)
        {
            this.userService = userService;
        }

        //Function to create a new user
        [HttpPost("")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            // Implementation for creating a new user
            
            var createdUser = await userService.Create(request);
           
            return CreatedAtAction(nameof(GetUsers), new { id = createdUser.Id }, createdUser);
        }

        
        //Function to update an existing user
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await userService.Update(id, request);
            return Ok(user);
        }


        //Function to get a user by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetById(id);
            if (user == null)
            {
                return NotFound($"User with id {id} not found.");
            }

            var response = new APIResponse<User>
            {
                success = true,
                message = "User retrieved successfully.",
                data = user
            };
            return Ok(response);
        }

        //Function to index all users
        [HttpGet("")]
        public async Task<IActionResult> GetUsers(int page = 1, int pageSize = 10, string? search = null)
        {
            var users = await userService.GetUsers(page, pageSize, search);

            var response = new APIResponse<IEnumerable<Models.User>>
            {
                success = true,
                message = "Users retrieved successfully.",
                data = users.Items,
                pagination = users.pagination
            };

            return Ok(response);
        }

        //function Soft delete user
        [HttpDelete("softDelete")]
        public async Task<IActionResult> SoftDeleteUser(int id)
        {
            var success = await userService.SoftDelete(id);
            
            var response = new APIResponse<Models.User>
            {
                success = success,
                message = success ? "Deleted Successfully" : "Deleted failed"
            };
            if (success)
            {
                return Ok(response);
            }else{
                return BadRequest(response);
            }
            
        }
    }
}