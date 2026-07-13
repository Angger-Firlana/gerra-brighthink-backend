using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.Task
{
    public class TaskFilteringRequest
    {
        public string? search {get; set;}
        public string? status {get; set;}
        public string? priority {get; set;}
        public int goalId {get; set;}
        public int page {get; set;}
        public int pageSize {get;set;}

    }
}