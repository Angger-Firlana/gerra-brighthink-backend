using backend.Data;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Role;

public interface IRoleService
{
    Task<List<Models.Role>> GetAll();
    Task<Models.Role?> GetById(int id);
}

public class RoleService : IRoleService
{
    private readonly AppDbContext dbContext;
    public RoleService(AppDbContext dbContext) => this.dbContext = dbContext;

    public async Task<List<Models.Role>> GetAll()
    {
        return await dbContext.Roles.OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<Models.Role?> GetById(int id)
    {
        return await dbContext.Roles.FirstOrDefaultAsync(r => r.Id == id);
    }
}
