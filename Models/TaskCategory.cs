using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class TaskCategory
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public User? User {get; set;}
        public string Name {get; set;} = string.Empty;
    }
}