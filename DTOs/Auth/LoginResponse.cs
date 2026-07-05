using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.DTOs.Auth
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public required Models.User user { get; set; }
        public required DateTime expiredAt { get; set; }

    }
}