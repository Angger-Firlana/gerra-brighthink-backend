using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Enum;

namespace backend.Models
{
    public class ActivityLog
    {
        public int Id {get; set;}
        public int UserId {get; set;}
        public User? Actor {get; set;}
        public int EntityId {get; set;}
        public EntityType EntityType {get; set;}    
        public string Action {get; set;} = string.Empty;
        public DateTime? created_at {get; set;}
        public DateTime? updated_at {get; set;}
        public DateTime? deleted_at {get; set;}
    }
}