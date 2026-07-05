using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Enum;

namespace backend.Models
{
    public class Task
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public User? User {get; set;}
        public int? GoalId {get; set;}
        public Goal? Goal {get; set;}
        public int? TaskCategoryId {get; set;}
        public TaskCategory? TaskCategory {get; set;}
        public string Title {get; set;} = string.Empty;
        public string Description {get; set;} = string.Empty;
        public TodoStatus Status {get; set;} = TodoStatus.NotStarted;
        public string Priority {get; set;} = string.Empty;
        public string TypeHabbit {get; set;} = string.Empty;
        public DateTime? created_at {get; set;}
        public DateTime? updated_at {get; set;}  
        public DateTime? deleted_at {get; set;}

    }
}