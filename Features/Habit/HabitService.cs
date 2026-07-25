using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Habit;
using backend.Enum;
using backend.Features.ActivityLog;
using backend.Wrapper;
using Dapper;

namespace backend.Features.Habit;

public class HabitService : IHabitService
{
    private readonly DapperContext _context;
    private readonly IActivityLogService activityLog;

    public HabitService(DapperContext context, ActivityLog.IActivityLogService activityLog)
    {
        _context = context;
        this.activityLog = activityLog;
    }

    public async Task<PagedResult<Models.Habit>> Index(HabitFilteringRequest filter, int userId)
    {
        using var db = _context.CreateConnection();

        var where = new List<string> { "h.deleted_at IS NULL", "h.UserId = @UserId" };
        var pars = new DynamicParameters();
        pars.Add("UserId", userId);

        if (!string.IsNullOrWhiteSpace(filter.search))
        {
            where.Add("h.Title LIKE @Search");
            pars.Add("Search", $"%{filter.search}%");
        }
        if (!string.IsNullOrWhiteSpace(filter.status) && int.TryParse(filter.status, out var status))
        {
            where.Add("h.Status = @Status");
            pars.Add("Status", status);
        }
        if (!string.IsNullOrWhiteSpace(filter.period))
        {
            where.Add("h.Period = @Period");
            pars.Add("Period", filter.period);
        }

        var whereClause = string.Join(" AND ", where);

        var totalItems = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM Habits h WHERE {whereClause}", pars);

        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;
        var offset = (page - 1) * pageSize;

        pars.Add("Limit", pageSize);
        pars.Add("Offset", offset);

        var habits = (await db.QueryAsync<Models.Habit>(
            $"SELECT * FROM Habits h WHERE {whereClause} ORDER BY h.created_at DESC LIMIT @Limit OFFSET @Offset",
            pars)).AsList();

        return new PagedResult<Models.Habit>
        {
            Items = habits,
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

    public async Task<Models.Habit?> GetById(int id, int userId)
    {
        using var db = _context.CreateConnection();
        return await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });
    }

    public async Task<Models.Habit> Create(CreateHabitRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Habit title cannot be null or empty.");

        using var db = _context.CreateConnection();

        var id = await db.ExecuteScalarAsync<int>(
            @"INSERT INTO Habits (UserId, Title, Period, TargetMinutes, Status, created_at, updated_at)
              VALUES (@UserId, @Title, @Period, @TargetMinutes, @Status, NOW(), NOW());
              SELECT LAST_INSERT_ID();",
            new
            {
                UserId = userId,
                request.Title,
                request.Period,
                request.TargetMinutes,
                Status = (int)request.Status
            });

        var habit = (await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id", new { Id = id }))!;

        await activityLog.Log(userId, habit.Id, EntityType.Habit, "created");

        return habit;
    }

    public async Task<Models.Habit?> Update(int id, UpdateHabitRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var existing = await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });

        if (existing is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        var sets = new List<string>();
        var pars = new DynamicParameters();
        pars.Add("Id", id);

        if (!string.IsNullOrWhiteSpace(request.Title)) { sets.Add("Title = @Title"); pars.Add("Title", request.Title); }
        if (!string.IsNullOrWhiteSpace(request.Period)) { sets.Add("Period = @Period"); pars.Add("Period", request.Period); }
        if (request.TargetMinutes.HasValue) { sets.Add("TargetMinutes = @TargetMinutes"); pars.Add("TargetMinutes", request.TargetMinutes.Value); }
        if (request.CurrentMinutes.HasValue) { sets.Add("CurrentMinutes = @CurrentMinutes"); pars.Add("CurrentMinutes", request.CurrentMinutes.Value); }
        if (request.Status.HasValue) { sets.Add("Status = @Status"); pars.Add("Status", (int)request.Status.Value); }

        if (sets.Count == 0)
        {
            await activityLog.Log(userId, id, EntityType.Habit, "updated");
            return existing;
        }

        sets.Add("updated_at = NOW()");
        await db.ExecuteAsync($"UPDATE Habits SET {string.Join(", ", sets)} WHERE Id = @Id", pars);

        await activityLog.Log(userId, id, EntityType.Habit, "updated");

        return await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        await db.ExecuteAsync("UPDATE Habits SET deleted_at = NOW() WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Habit, "deleted");
        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Habit>(
            "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        await db.ExecuteAsync("DELETE FROM Habits WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Habit, "deleted_hard");
        return true;
    }
}
