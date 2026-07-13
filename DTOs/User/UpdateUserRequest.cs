using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.User
{
    public class UpdateUserRequest
    {
        public string? Username {get; set;} = string.Empty;  
        public string? Name {get; set;} = string.Empty;
        public string? Email {get; set;} = string.Empty;
        public string? Password {get; set;} = string.Empty;
        public bool? IsActive {get; set;}
        public int? RoleId {get; set;}
    }
}