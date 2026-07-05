using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.Auth
{
    public class LoginRequest
    {
        public required string identity {get; set;}
        public required string password {get; set;}
    }
}