using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Enum;

namespace backend.Models
{
    public class Habit
    {
        public int Id {get; set;}
        public int userId {get; set;}
        public User? User {get; set;}
        public string Title {get; set;} = string.Empty;
        public string Period {get; set;} = string.Empty;
        public int TargetMinutes {get; set;}
        public int CurrentMinutes {get; set;}
        public HabitStatus Status {get; set;}
        public DateTime? CurrentStartedAt {get; set;}
        public DateTime? created_at {get; set;}
        public DateTime? updated_at {get; set;}  
        public DateTime? deleted_at {get; set;}
    }
}