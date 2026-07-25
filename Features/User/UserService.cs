using System.Data;
using Dapper;
using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.User;
using backend.Wrapper;

namespace backend.Features.User;

public class UserService : IUserService
{
    private readonly DapperContext _context;
    public UserService(DapperContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Models.User>> GetUsers(int page, int pageSize, string? search = null)
    {
        using var db = _context.CreateConnection();

        var baseWhere = "WHERE u.deleted_at IS NULL";
        if (!string.IsNullOrWhiteSpace(search))
        {
            baseWhere += " AND (u.Username LIKE @Search OR u.Name LIKE @Search)";
        }

        var countSql = $"SELECT COUNT(*) FROM Users u {baseWhere}";
        var totalItems = await db.ExecuteScalarAsync<int>(countSql, new { Search = $"%{search}%" });

        var offset = (page - 1) * pageSize;
        var dataSql = $@"
            SELECT u.*, r.Id, r.Name
            FROM Users u
            LEFT JOIN Roles r ON r.Id = u.RoleId
            {baseWhere}
            ORDER BY u.Id
            LIMIT @Limit OFFSET @Offset";

        var users = (await db.QueryAsync<Models.User, Models.Role, Models.User>(
            dataSql,
            (user, role) => { user.Role = role; return user; },
            new { Search = $"%{search}%", Limit = pageSize, Offset = offset },
            splitOn: "Id"
        )).AsList();

        return new PagedResult<Models.User>
        {
            Items = users,
            pagination = new Pagination
            {
                page = page,
                pageSize = pageSize,
                total = totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                hasNextPage = page * pageSize < totalItems,
                hasPreviousPage = page > 1
            }
        };
    }

    public async Task<Models.User> Create(CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Name) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Username, Name, Email, and Password cannot be null or empty.");
        }

        var password = BCrypt.Net.BCrypt.HashPassword(request.Password);

        using var db = _context.CreateConnection();
        var sql = @"
            INSERT INTO Users (Username, Name, Email, Password, isActive, RoleId, created_at, updated_at)
            VALUES (@Username, @Name, @Email, @Password, @IsActive, @RoleId, NOW(), NOW());
            SELECT LAST_INSERT_ID();";

        var id = await db.ExecuteScalarAsync<int>(sql, new
        {
            request.Username,
            request.Name,
            request.Email,
            Password = password,
            request.IsActive,
            request.RoleId
        });

        return (await db.QueryFirstOrDefaultAsync<Models.User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id }))!;
    }

    public async Task<Models.User?> GetById(int id)
    {
        using var db = _context.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Models.User>(
            "SELECT * FROM Users WHERE Id = @Id AND deleted_at IS NULL",
            new { Id = id }
        );
    }

    public async Task<Models.User> Update(int id, UpdateUserRequest request)
    {
        using var db = _context.CreateConnection();

        var existing = await db.QueryFirstOrDefaultAsync<Models.User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        if (existing == null)
            throw new ArgumentException($"User with id {id} not found.");

        var sets = new List<string>();
        var pars = new DynamicParameters();
        pars.Add("Id", id);

        if (!string.IsNullOrEmpty(request.Username)) { sets.Add("Username = @Username"); pars.Add("Username", request.Username); }
        if (!string.IsNullOrEmpty(request.Name)) { sets.Add("Name = @Name"); pars.Add("Name", request.Name); }
        if (!string.IsNullOrEmpty(request.Email)) { sets.Add("Email = @Email"); pars.Add("Email", request.Email); }
        if (!string.IsNullOrEmpty(request.Password)) { sets.Add("Password = @Password"); pars.Add("Password", BCrypt.Net.BCrypt.HashPassword(request.Password)); }
        if (request.IsActive.HasValue) { sets.Add("isActive = @IsActive"); pars.Add("IsActive", request.IsActive.Value); }
        if (request.RoleId.HasValue) { sets.Add("RoleId = @RoleId"); pars.Add("RoleId", request.RoleId.Value); }

        if (sets.Count == 0)
            return existing;

        sets.Add("updated_at = NOW()");
        var sql = $"UPDATE Users SET {string.Join(", ", sets)} WHERE Id = @Id";

        await db.ExecuteAsync(sql, pars);

        return (await db.QueryFirstOrDefaultAsync<Models.User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id }))!;
    }

    public async Task<bool> SoftDelete(int id)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        if (existing == null)
            throw new ArgumentException($"User with id {id} not found.");

        var rows = await db.ExecuteAsync(
            "UPDATE Users SET deleted_at = NOW() WHERE Id = @Id", new { Id = id });
        return rows > 0;
    }

    public async Task<bool> Delete(int id)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.User>(
            "SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        if (existing == null)
            throw new ArgumentException($"User with id {id} not found.");

        var rows = await db.ExecuteAsync("DELETE FROM Users WHERE Id = @Id", new { Id = id });
        return rows > 0;
    }
}
