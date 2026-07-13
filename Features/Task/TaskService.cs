using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Task;
using backend.Wrapper;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Task;

public class TaskService : ITaskService
{
    private readonly AppDbContext dbContext;

    public TaskService(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<PagedResult<Models.Task>> Index(TaskFilteringRequest filter, int userId)
    {
        var query = dbContext.Tasks
            .Include(t => t.TaskCategory)
            .Include(t => t.Goal)
            .Include(t => t.SubTasks)
            .Where(t => t.UserId == userId && t.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.search))
            query = query.Where(t => t.Title.Contains(filter.search) || t.Description!.Contains(filter.search));

        if (!string.IsNullOrWhiteSpace(filter.status) && int.TryParse(filter.status, out var status))
            query = query.Where(t => (int)t.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.priority))
            query = query.Where(t => t.Priority == filter.priority);

        if (filter.goalId > 0)
            query = query.Where(t => t.GoalId == filter.goalId);

        var totalItems = await query.CountAsync();
        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Models.Task>
        {
            Items = tasks,
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

    public async Task<Models.Task?> GetById(int id, int userId)
    {
        return await dbContext.Tasks
            .Include(t => t.TaskCategory)
            .Include(t => t.Goal)
            .Include(t => t.SubTasks!.Where(s => s.DeletedAt == null))
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);
    }

    public async Task<Models.Task> Create(CreateTaskRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be null or empty.");

        var task = new Models.Task
        {
            UserId = userId,
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Status = request.Status,
            Priority = request.Priority,
            TypeHabbit = request.TypeHabbit ?? string.Empty,
            GoalId = request.GoalId,
            TaskCategoryId = request.TaskCategoryId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.SubTasks?.Count > 0)
        {
            task.SubTasks = request.SubTasks.Select(s => new Models.SubTask
            {
                Title = s.Title,
                Status = s.Status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
        }

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<Models.Task?> Update(int id, UpdateTaskRequest request, int userId)
    {
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);

        if (task is null)
            throw new ArgumentException($"Task with id {id} not found.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            task.Title = request.Title;
        if (request.Description is not null)
            task.Description = request.Description;
        if (request.Status.HasValue)
            task.Status = request.Status.Value;
        if (!string.IsNullOrWhiteSpace(request.Priority))
            task.Priority = request.Priority;
        if (request.TypeHabbit is not null)
            task.TypeHabbit = request.TypeHabbit;
        if (request.GoalId.HasValue)
            task.GoalId = request.GoalId;
        if (request.TaskCategoryId.HasValue)
            task.TaskCategoryId = request.TaskCategoryId;

        task.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return task;
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && t.DeletedAt == null);

        if (task is null)
            throw new ArgumentException($"Task with id {id} not found.");

        task.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task is null)
            throw new ArgumentException($"Task with id {id} not found.");

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();
        return true;
    }

    // --- SubTask ---

    public async Task<Models.SubTask> AddSubTask(int taskId, CreateSubTaskRequest request, int userId)
    {
        var task = await dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == taskId && t.UserId == userId && t.DeletedAt == null);

        if (task is null)
            throw new ArgumentException($"Task with id {taskId} not found.");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("SubTask title cannot be null or empty.");

        var subTask = new Models.SubTask
        {
            TaskId = taskId,
            Title = request.Title,
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.SubTasks.Add(subTask);
        await dbContext.SaveChangesAsync();

        return subTask;
    }

    public async Task<Models.SubTask?> UpdateSubTask(int subTaskId, UpdateSubTaskRequest request, int userId)
    {
        var subTask = await dbContext.SubTasks
            .Include(s => s.Task)
            .FirstOrDefaultAsync(s => s.Id == subTaskId && s.DeletedAt == null && s.Task!.UserId == userId);

        if (subTask is null)
            throw new ArgumentException($"SubTask with id {subTaskId} not found.");

        if (!string.IsNullOrWhiteSpace(request.Title))
            subTask.Title = request.Title;
        if (request.Status.HasValue)
            subTask.Status = request.Status.Value;

        subTask.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return subTask;
    }

    public async Task<bool> DeleteSubTask(int subTaskId, int userId)
    {
        var subTask = await dbContext.SubTasks
            .Include(s => s.Task)
            .FirstOrDefaultAsync(s => s.Id == subTaskId && s.DeletedAt == null && s.Task!.UserId == userId);

        if (subTask is null)
            throw new ArgumentException($"SubTask with id {subTaskId} not found.");

        subTask.DeletedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }
}
