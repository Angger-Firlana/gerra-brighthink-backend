using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Goal;
using backend.Enum;
using backend.Features.ActivityLog;
using backend.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Goal;

public class GoalService : IGoalService
{
    private readonly AppDbContext dbContext;
    private readonly IActivityLogService activityLog;

    public GoalService(AppDbContext dbContext, IActivityLogService activityLog)
    {
        this.dbContext = dbContext;
        this.activityLog = activityLog;
    }

    public async Task<PagedResult<Models.Goal>> Index(GoalFilteringRequest filter, int userId)
    {
        var baseQuery = dbContext.Goals
            .Where(g => g.UserId == userId && g.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filter.search))
            baseQuery = baseQuery.Where(g => g.Title.Contains(filter.search));

        if (!string.IsNullOrWhiteSpace(filter.status) && int.TryParse(filter.status, out var status))
            baseQuery = baseQuery.Where(g => (int)g.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.typeGoal))
            baseQuery = baseQuery.Where(g => g.TypeGoal == filter.typeGoal);

        var totalItems = await baseQuery.CountAsync();
        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;

        // Single query: select Goal + task count from DB without loading Tasks into memory
        var goalIds = await baseQuery
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => g.Id)
            .ToListAsync();

        var taskCounts = await dbContext.Tasks
            .Where(t => goalIds.Contains(t.GoalId!.Value) && t.DeletedAt == null)
            .GroupBy(t => t.GoalId!.Value)
            .Select(g => new { GoalId = g.Key, Total = g.Count(), Done = g.Count(t => t.Status == TodoStatus.Completed) })
            .ToDictionaryAsync(x => x.GoalId, x => (x.Total, x.Done));

        var goals = await dbContext.Goals
            .Where(g => goalIds.Contains(g.Id))
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var result = goals.Select(g =>
        {
            var (total, done) = taskCounts.TryGetValue(g.Id, out var t) ? t : (0, 0);
            g.TaskCount = total;
            g.DoneCount = done;
            g.Tasks = null;
            return g;
        }).ToList();

        return new PagedResult<Models.Goal>
        {
            Items = result,
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
        return await dbContext.Goals
            .Include(g => g.Tasks!.Where(t => t.DeletedAt == null))
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId && g.DeletedAt == null);
    }

    public async Task<Models.Goal> Create(CreateGoalRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Goal title cannot be null or empty.");

        var goal = new Models.Goal
        {
            UserId = userId,
            Title = request.Title,
            TypeGoal = request.TypeGoal,
            Status = request.Status,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Goals.Add(goal);
        await dbContext.SaveChangesAsync();

        await activityLog.Log(userId, goal.Id, EntityType.Goal, "created");

        return goal;
    }

    public async Task<Models.Goal?> Update(int id, UpdateGoalRequest request, int userId)
    {
        var goal = await dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId && g.DeletedAt == null);

        if (goal is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            goal.Title = request.Title;
        if (!string.IsNullOrWhiteSpace(request.TypeGoal))
            goal.TypeGoal = request.TypeGoal;
        if (request.Status.HasValue)
            goal.Status = request.Status.Value;
        if (request.DueDate.HasValue)
            goal.DueDate = request.DueDate;

        goal.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await activityLog.Log(userId, id, EntityType.Goal, "updated");

        return goal;
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        var goal = await dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId && g.DeletedAt == null);

        if (goal is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        goal.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        await activityLog.Log(userId, id, EntityType.Goal, "deleted");

        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var goal = await dbContext.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        if (goal is null)
            throw new ArgumentException($"Goal with id {id} not found.");

        dbContext.Goals.Remove(goal);
        await dbContext.SaveChangesAsync();

        await activityLog.Log(userId, id, EntityType.Goal, "deleted_hard");

        return true;
    }
}
