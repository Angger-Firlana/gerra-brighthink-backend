using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Goal;
using backend.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Goal;

public class GoalService : IGoalService
{
    private readonly AppDbContext dbContext;

    public GoalService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<Models.Goal>> Index(GoalFilteringRequest filter, int userId)
    {
        var query = dbContext.Goals
            .Where(g => g.UserId == userId && g.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.search))
            query = query.Where(g => g.Title.Contains(filter.search));

        if (!string.IsNullOrWhiteSpace(filter.status) && int.TryParse(filter.status, out var status))
            query = query.Where(g => (int)g.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.typeGoal))
            query = query.Where(g => g.TypeGoal == filter.typeGoal);

        var totalItems = await query.CountAsync();
        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;

        var goals = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
        return await dbContext.Goals
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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Goals.Add(goal);
        await dbContext.SaveChangesAsync();

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

        goal.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

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
        return true;
    }
}
