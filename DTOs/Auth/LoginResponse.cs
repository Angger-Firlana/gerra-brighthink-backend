using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;

namespace backend.DTOs.Auth
{
    public class LoginResponse
    {
        public string? token { get; set; }
        public int code {get; set;}
        public string? message {get; set;}
        public Models.User? user { get; set; }
        public DateTime expiredAt { get; set; }

    }
}