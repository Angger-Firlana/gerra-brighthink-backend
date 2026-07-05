using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Enum;

namespace backend.Models
{
    public class SubTask
    {
        public int Id {get; set;}
        public int TaskId {get; set;}
        public Task? Task {get; set;}
        public string Title {get; set;} = string.Empty;
        public SubTaskStatus Status {get; set;} = SubTaskStatus.NotStarted;
        public DateTime? created_at {get; set;}
        public DateTime? updated_at {get; set;}
        public DateTime? deleted_at {get; set;}
    }
}