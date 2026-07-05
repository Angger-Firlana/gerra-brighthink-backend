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

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context ;
        private readonly IUserService userService;

        public UserController(AppDbContext context, IUserService userService)
        {
            _context = context;
            this.userService = userService;
        }
        // [HttpGet("/")]
        // public IActionResult GetUsers()
        // {
        //     // Implementation for getting users

        // }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDB()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                if (canConnect)
                {
                    return Ok("Database connection successful.");
                }
                else
                {
                    return StatusCode(500, "Database connection failed.");
                }
            }catch(Exception ex)
            {
                return StatusCode(500, $"Database connection failed: {ex}");
            }
        }

        [HttpPost("")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            // Implementation for creating a new user
            
            var user = await userService.Create(request);
           
            var createdUser = await _context.Users.Include(u=> u.Role).FirstOrDefaultAsync(u => u.Id == user.Id);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, createdUser);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var user = await userService.Update(id, request);
            return Ok(user);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await userService.GetById(id);
            if (user == null)
            {
                return NotFound($"User with id {id} not found.");
            }
            return Ok(user);
        }

        [HttpGet("")]
        public IActionResult GetUsers(int page = 1, int pageSize = 10, string? search = null)
        {
            var users = _context.Users.Where(
                u=> u.Username.Contains(search ?? "") || 
                u.Name.Contains(search ?? "")
                )
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(users);
        }
    }
}