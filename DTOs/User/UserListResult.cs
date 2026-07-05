using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Api;
using backend.Models;

namespace backend.DTOs.User
{
    public class UserListResult
    {
        public IEnumerable<Models.User> users {get; set;} = [];
        public Pagination pagination {get; set;} = new();
    }
}