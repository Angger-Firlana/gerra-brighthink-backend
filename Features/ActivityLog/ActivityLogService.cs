using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.ActivityLog;
using backend.Enum;
using backend.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.ActivityLog;

public class ActivityLogService : IActivityLogService
{
    private readonly AppDbContext dbContext;

    public ActivityLogService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async System.Threading.Tasks.Task<PagedResult<Models.ActivityLog>> Index(ActivityLogFilteringRequest filter, int userId)
    {
        var query = dbContext.ActivityLogs
            .Include(a => a.Actor)
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.action))
            query = query.Where(a => a.Action == filter.action);

        if (filter.entityType.HasValue)
            query = query.Where(a => (int)a.EntityType == filter.entityType.Value);

        var totalItems = await query.CountAsync();
        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 20;

        var logs = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
        var log = new Models.ActivityLog
        {
            UserId = userId,
            EntityId = entityId,
            EntityType = entityType,
            Action = action,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.ActivityLogs.Add(log);
        await dbContext.SaveChangesAsync();
    }
}
