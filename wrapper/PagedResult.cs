using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.DTOs.Api;

namespace backend.wrapper
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items {get; set;} = [];
        public Pagination pagination {get; set;} = new();
    }
}