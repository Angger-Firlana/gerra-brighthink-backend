using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Habit;
using backend.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Habit;

public class HabitService : IHabitService
{
    private readonly AppDbContext dbContext;

    public HabitService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<Models.Habit>> Index(HabitFilteringRequest filter, int userId)
    {
        var query = dbContext.Habits
            .Where(h => h.UserId == userId && h.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.search))
            query = query.Where(h => h.Title.Contains(filter.search));

        if (!string.IsNullOrWhiteSpace(filter.status) && int.TryParse(filter.status, out var status))
            query = query.Where(h => (int)h.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.period))
            query = query.Where(h => h.Period == filter.period);

        var totalItems = await query.CountAsync();
        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;

        var habits = await query
            .OrderByDescending(h => h.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

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
        return await dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && h.DeletedAt == null);
    }

    public async Task<Models.Habit> Create(CreateHabitRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Habit title cannot be null or empty.");

        var habit = new Models.Habit
        {
            UserId = userId,
            Title = request.Title,
            Period = request.Period,
            TargetMinutes = request.TargetMinutes,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();

        return habit;
    }

    public async Task<Models.Habit?> Update(int id, UpdateHabitRequest request, int userId)
    {
        var habit = await dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && h.DeletedAt == null);

        if (habit is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            habit.Title = request.Title;
        if (!string.IsNullOrWhiteSpace(request.Period))
            habit.Period = request.Period;
        if (request.TargetMinutes.HasValue)
            habit.TargetMinutes = request.TargetMinutes.Value;
        if (request.CurrentMinutes.HasValue)
            habit.CurrentMinutes = request.CurrentMinutes.Value;
        if (request.Status.HasValue)
            habit.Status = request.Status.Value;

        habit.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return habit;
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        var habit = await dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId && h.DeletedAt == null);

        if (habit is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        habit.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var habit = await dbContext.Habits
            .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

        if (habit is null)
            throw new ArgumentException($"Habit with id {id} not found.");

        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return true;
    }
}
