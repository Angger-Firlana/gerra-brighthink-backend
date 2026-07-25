using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Goal;
using backend.Enum;
using backend.Features.ActivityLog;
using backend.Helpers;
using backend.Wrapper;
using Dapper;

namespace backend.Features.Goal;

public class GoalService : IGoalService
{
    private readonly DapperContext _context;
    private readonly IActivityLogService activityLog;

    public GoalService(DapperContext context, ActivityLog.IActivityLogService activityLog)
    {
        _context = context;
        this.activityLog = activityLog;
    }

    public async Task<PagedResult<Models.Goal>> Index(GoalFilteringRequest filter, int userId)
    {
        using var db = _context.CreateConnection();

        var (whereClause, pars) = SqlHelper.Where("g.deleted_at IS NULL")
            .Add("g.UserId = @UserId", "UserId", userId)
            .Add("g.Title LIKE @Search", "Search", filter.search is { Length: >0 } s ? $"%{s}%" : null)
            .AddInt("g.Status", "Status", filter.status)
            .Add("g.TypeGoal = @TypeGoal", "TypeGoal", filter.typeGoal)
            .Build();

        var totalItems = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM Goals g WHERE {whereClause}", pars);

        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;
        var offset = (page - 1) * pageSize;

        pars.Add("Limit", pageSize);
        pars.Add("Offset", offset);

        var goals = (await db.QueryAsync<Models.Goal>(
            $"SELECT * FROM Goals g WHERE {whereClause} ORDER BY g.created_at DESC LIMIT @Limit OFFSET @Offset",
            pars)).AsList();

        // Attach TaskCount and DoneCount per goal
        if (goals.Count > 0)
        {
            var goalIds = goals.Select(g => g.Id).ToList();
            var counts = (await db.QueryAsync(
                @"SELECT GoalId AS Id, COUNT(*) AS Total, SUM(CASE WHEN Status = @Done THEN 1 ELSE 0 END) AS Done
                  FROM Tasks
                  WHERE GoalId IN @Ids AND deleted_at IS NULL
                  GROUP BY GoalId",
                new { Done = (int)TodoStatus.Completed, Ids = goalIds })).ToDictionary(r => (int)r.Id, r => (Total: (int)r.Total, Done: (int)r.Done));

            foreach (var g in goals)
            {
                if (counts.TryGetValue(g.Id, out var c))
                {
                    g.TaskCount = c.Total;
                    g.DoneCount = c.Done;
                }
            }
        }

        return new PagedResult<Models.Goal>
        {
            Items = goals,
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

    public async Task<Models.Goal?> GetById(int id, int userId)
    {
        using var db = _context.CreateConnection();

        var goal = await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });

        if (goal is null) return null;

        goal.Tasks = (await db.QueryAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE GoalId = @GoalId AND deleted_at IS NULL ORDER BY Id",
            new { GoalId = id })).AsList();

        return goal;
    }

    public async Task<Models.Goal> Create(CreateGoalRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Goal title cannot be null or empty.");

        using var db = _context.CreateConnection();

        var id = await db.ExecuteScalarAsync<int>(
            @"INSERT INTO Goals (UserId, Title, TypeGoal, Status, DueDate, created_at, updated_at)
              VALUES (@UserId, @Title, @TypeGoal, @Status, @DueDate, NOW(), NOW());
              SELECT LAST_INSERT_ID();",
            new
            {
                UserId = userId,
                request.Title,
                request.TypeGoal,
                Status = (int)request.Status,
                request.DueDate
            });

        var goal = (await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id", new { Id = id }))!;

        await activityLog.Log(userId, goal.Id, EntityType.Goal, "created");

        return goal;
    }

    public async Task<Models.Goal?> Update(int id, UpdateGoalRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var existing = await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });

        if (existing is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        var (sql, pars) = SqlHelper.Update("Goals")
            .Set("Title", "Title", request.Title is { Length: >0 } t ? t : null)
            .Set("TypeGoal", "TypeGoal", request.TypeGoal is { Length: >0 } ty ? ty : null)
            .Set("Status", "Status", request.Status.HasValue ? (int)request.Status.Value : null)
            .Set("DueDate", "DueDate", request.DueDate)
            .Where("Id = @Id", new { Id = id })
            .Build();

        await db.ExecuteAsync(sql, pars);
        await activityLog.Log(userId, id, EntityType.Goal, "updated");

        return await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        await db.ExecuteAsync("UPDATE Goals SET deleted_at = NOW() WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Goal, "deleted");
        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        await db.ExecuteAsync("DELETE FROM Goals WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Goal, "deleted_hard");
        return true;
    }
}
