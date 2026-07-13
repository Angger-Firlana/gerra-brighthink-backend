using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.Api
{
    public class Pagination
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public int total { get; set; }
        public int totalPages { get; set; }
        public bool hasNextPage { get; set; }
        public bool hasPreviousPage { get; set; }
    }
}