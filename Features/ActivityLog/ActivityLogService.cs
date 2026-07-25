using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.ActivityLog;
using backend.Enum;
using backend.Wrapper;
using Dapper;

namespace backend.Features.ActivityLog;

public class ActivityLogService : IActivityLogService
{
    private readonly DapperContext _context;

    public ActivityLogService(DapperContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<PagedResult<Models.ActivityLog>> Index(ActivityLogFilteringRequest filter, int userId)
    {
        using var db = _context.CreateConnection();

        var where = new List<string> { "a.deleted_at IS NULL", "a.UserId = @UserId" };
        var pars = new DynamicParameters();
        pars.Add("UserId", userId);

        if (!string.IsNullOrWhiteSpace(filter.action))
        {
            where.Add("a.Action = @Action");
            pars.Add("Action", filter.action);
        }
        if (filter.entityType.HasValue)
        {
            where.Add("a.EntityType = @EntityType");
            pars.Add("EntityType", filter.entityType.Value);
        }

        var whereClause = string.Join(" AND ", where);

        var totalItems = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM ActivityLogs a WHERE {whereClause}", pars);

        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 20;
        var offset = (page - 1) * pageSize;

        pars.Add("Limit", pageSize);
        pars.Add("Offset", offset);

        var logs = (await db.QueryAsync<Models.ActivityLog>(
            $"SELECT * FROM ActivityLogs a WHERE {whereClause} ORDER BY a.created_at DESC LIMIT @Limit OFFSET @Offset",
            pars)).AsList();

        // Load Actor (User) for each log
        var userIds = logs.Select(l => l.UserId).Distinct().ToList();
        if (userIds.Count > 0)
        {
            var users = (await db.QueryAsync<Models.User>(
                "SELECT * FROM Users WHERE Id IN @Ids", new { Ids = userIds }))
                .ToDictionary(u => u.Id);

            foreach (var log in logs)
            {
                if (users.TryGetValue(log.UserId, out var actor))
                    log.Actor = actor;
            }
        }

        return new PagedResult<Models.ActivityLog>
        {
            Items = logs,
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

    public async System.Threading.Tasks.Task Log(int userId, int entityId, EntityType entityType, string action)
    {
        using var db = _context.CreateConnection();
        await db.ExecuteAsync(
            @"INSERT INTO ActivityLogs (UserId, EntityId, EntityType, Action, created_at)
              VALUES (@UserId, @EntityId, @EntityType, @Action, NOW())",
            new { UserId = userId, EntityId = entityId, EntityType = (int)entityType, Action = action });
    }
}
