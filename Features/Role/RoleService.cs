using backend.Data;
using Dapper;

namespace backend.Features.Role;

public interface IRoleService
{
    Task<List<Models.Role>> GetAll();
    Task<Models.Role?> GetById(int id);
}

public class RoleService : IRoleService
{
    private readonly DapperContext _context;
    public RoleService(DapperContext context) => _context = context;

    public async Task<List<Models.Role>> GetAll()
    {
        using var db = _context.CreateConnection();
        var roles = await db.QueryAsync<Models.Role>("SELECT * FROM Roles ORDER BY Id");
        return roles.AsList();
    }

    public async Task<Models.Role?> GetById(int id)
    {
        using var db = _context.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Models.Role>(
            "SELECT * FROM Roles WHERE Id = @Id", new { Id = id });
    }
}
