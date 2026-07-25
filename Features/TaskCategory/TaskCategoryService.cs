using backend.Data;
using Dapper;

namespace backend.Features.TaskCategory;

public class TaskCategoryService : ITaskCategoryService
{
    private readonly DapperContext _context;

    public TaskCategoryService(DapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Models.TaskCategory>> Index(int userId)
    {
        using var db = _context.CreateConnection();
        return await db.QueryAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE UserId = @UserId ORDER BY Name",
            new { UserId = userId });
    }

    public async Task<Models.TaskCategory?> GetById(int id, int userId)
    {
        using var db = _context.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id AND UserId = @UserId",
            new { Id = id, UserId = userId });
    }

    public async Task<Models.TaskCategory> Create(DTOs.TaskCategory.CreateTaskCategoryRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Category name cannot be null or empty.");

        using var db = _context.CreateConnection();

        var id = await db.ExecuteScalarAsync<int>(
            "INSERT INTO TaskCategories (UserId, Name) VALUES (@UserId, @Name); SELECT LAST_INSERT_ID();",
            new { UserId = userId, request.Name });

        return (await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id", new { Id = id }))!;
    }

    public async Task<Models.TaskCategory?> Update(int id, DTOs.TaskCategory.UpdateTaskCategoryRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var existing = await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id AND UserId = @UserId",
            new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"TaskCategory with id {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            await db.ExecuteAsync("UPDATE TaskCategories SET Name = @Name WHERE Id = @Id",
                new { request.Name, Id = id });
        }

        return await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> Delete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id AND UserId = @UserId",
            new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"TaskCategory with id {id} not found.");

        await db.ExecuteAsync("DELETE FROM TaskCategories WHERE Id = @Id", new { Id = id });
        return true;
    }
}
