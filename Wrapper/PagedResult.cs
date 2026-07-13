using backend.DTOs.Api;

namespace backend.Wrapper;

public class PagedResult<T>
{
    public IEnumerable<T> Items {get; set;} = [];
    public Pagination pagination {get; set;} = new();
}
