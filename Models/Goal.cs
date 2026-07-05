using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Enum;

namespace backend.Models
{
    public class Goal
    {
        public int Id {get; set;}   
        public string TypeGoal {get; set;} = string.Empty;
        public int UserId {get; set;}
        public User? User {get; set;}
        public string Title {get; set;} = string.Empty;
        public GoalStatus Status {get; set;}    
        public DateTime? created_at {get; set;}
        public DateTime? updated_at {get; set;}  
        public DateTime? deleted_at {get; set;}
    }
}