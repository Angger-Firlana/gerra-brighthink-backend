using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs.Api
{
    public class APIResponse<T>
    {
        public bool success { get; set; } = true;
        public string? message { get; set; } = null;
        public T? data { get; set; } = default;
        public Pagination? pagination { get; set; } = null;

    }
}