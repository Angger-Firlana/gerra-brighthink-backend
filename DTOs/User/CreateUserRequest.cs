using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.User
{
    public class CreateUserRequest
    {
        public required string Username {get; set;}  
        public required string Name {get; set;}
        public required string Email {get; set;}
        public required string Password {get; set;}
        public bool IsActive {get; set;}
        public int RoleId {get; set;}
    }
}