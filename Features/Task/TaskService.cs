using backend.Data;
using backend.DTOs.Api;
using backend.DTOs.Task;
using backend.Enum;
using backend.Features.ActivityLog;
using backend.Helpers;
using backend.Wrapper;
using Dapper;

namespace backend.Features.Task;

public class TaskService : ITaskService
{
    private readonly DapperContext _context;
    private readonly IActivityLogService activityLog;

    public TaskService(DapperContext context, ActivityLog.IActivityLogService activityLog)
    {
        _context = context;
        this.activityLog = activityLog;
    }

    public async Task<PagedResult<Models.Task>> Index(TaskFilteringRequest filter, int userId)
    {
        using var db = _context.CreateConnection();

        var (whereClause, pars) = SqlHelper.Where("t.deleted_at IS NULL")
            .Add("t.UserId = @UserId", "UserId", userId)
            .Add("(t.Title LIKE @Search OR t.Description LIKE @Search)", "Search", filter.search is { Length: >0 } s ? $"%{s}%" : null)
            .AddInt("t.Status", "Status", filter.status)
            .Add("t.Priority = @Priority", "Priority", filter.priority)
            .Build();

        var totalItems = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM Tasks t WHERE {whereClause}", pars);

        var page = filter.page > 0 ? filter.page : 1;
        var pageSize = filter.pageSize > 0 ? filter.pageSize : 10;
        var offset = (page - 1) * pageSize;

        pars.Add("Limit", pageSize);
        pars.Add("Offset", offset);

        var tasks = (await db.QueryAsync<Models.Task>(
            $"SELECT t.* FROM Tasks t WHERE {whereClause} ORDER BY t.created_at DESC LIMIT @Limit OFFSET @Offset", pars)).AsList();

        var taskIds = tasks.Select(t => t.Id).ToList();

        if (taskIds.Count > 0)
        {
            var categories = (await db.QueryAsync<Models.TaskCategory>(
                "SELECT * FROM TaskCategories WHERE Id IN @Ids", new { Ids = taskIds })).ToDictionary(c => c.Id);
            var goals = (await db.QueryAsync<Models.Goal>(
                "SELECT * FROM Goals WHERE Id IN @Ids", new { Ids = taskIds })).ToDictionary(g => g.Id);
            var subTasks = (await db.QueryAsync<Models.SubTask>(
                "SELECT * FROM SubTasks WHERE TaskId IN @Ids AND deleted_at IS NULL", new { Ids = taskIds }))
                .GroupBy(s => s.TaskId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var t in tasks)
            {
                if (t.TaskCategoryId.HasValue && categories.TryGetValue(t.TaskCategoryId.Value, out var cat))
                    t.TaskCategory = cat;
                if (t.GoalId.HasValue && goals.TryGetValue(t.GoalId.Value, out var g))
                    t.Goal = g;
                if (subTasks.TryGetValue(t.Id, out var subs))
                    t.SubTasks = subs;
            }
        }

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
        using var db = _context.CreateConnection();

        var task = await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });

        if (task is null) return null;

        task.TaskCategory = await db.QueryFirstOrDefaultAsync<Models.TaskCategory>(
            "SELECT * FROM TaskCategories WHERE Id = @Id", new { Id = task.TaskCategoryId });
        task.Goal = await db.QueryFirstOrDefaultAsync<Models.Goal>(
            "SELECT * FROM Goals WHERE Id = @Id", new { Id = task.GoalId });
        task.SubTasks = (await db.QueryAsync<Models.SubTask>(
            "SELECT * FROM SubTasks WHERE TaskId = @TaskId AND deleted_at IS NULL ORDER BY Id",
            new { TaskId = id })).AsList();

        return task;
    }

    public async Task<Models.Task> Create(CreateTaskRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be null or empty.");

        using var db = _context.CreateConnection();

        var sql = @"
            INSERT INTO Tasks (UserId, Title, Description, Status, Priority, TypeHabbit, GoalId, TaskCategoryId, DueDate, StartTime, EndTime, created_at, updated_at)
            VALUES (@UserId, @Title, @Description, @Status, @Priority, @TypeHabbit, @GoalId, @TaskCategoryId, @DueDate, @StartTime, @EndTime, NOW(), NOW());
            SELECT LAST_INSERT_ID();";

        var id = await db.ExecuteScalarAsync<int>(sql, new
        {
            UserId = userId,
            request.Title,
            Description = request.Description ?? string.Empty,
            Status = (int)request.Status,
            request.Priority,
            TypeHabbit = request.TypeHabbit ?? string.Empty,
            request.GoalId,
            request.TaskCategoryId,
            request.DueDate,
            request.StartTime,
            request.EndTime
        });

        // Insert subTasks
        if (request.SubTasks?.Count > 0)
        {
            foreach (var st in request.SubTasks)
            {
                await db.ExecuteAsync(
                    @"INSERT INTO SubTasks (TaskId, Title, Status, created_at, updated_at)
                      VALUES (@TaskId, @Title, @Status, NOW(), NOW())",
                    new { TaskId = id, st.Title, Status = (int)st.Status });
            }
        }

        var task = (await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id", new { Id = id }))!;

        await activityLog.Log(userId, task.Id, EntityType.Task, "created");

        return task;
    }

    public async Task<Models.Task?> Update(int id, UpdateTaskRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var existing = await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });

        if (existing is null)
            throw new ArgumentException($"Task with id {id} not found.");

        var (sql, pars) = SqlHelper.Update("Tasks")
            .Set("Title", "Title", request.Title is { Length: >0 } t ? t : null)
            .Set("Description", "Description", request.Description)
            .Set("Status", "Status", request.Status.HasValue ? (int)request.Status.Value : null)
            .Set("Priority", "Priority", request.Priority is { Length: >0 } p ? p : null)
            .Set("TypeHabbit", "TypeHabbit", request.TypeHabbit)
            .Set("GoalId", "GoalId", request.GoalId.HasValue ? request.GoalId.Value : null)
            .Set("TaskCategoryId", "TaskCategoryId", request.TaskCategoryId.HasValue ? request.TaskCategoryId.Value : null)
            .Set("DueDate", "DueDate", request.DueDate)
            .Set("StartTime", "StartTime", request.StartTime)
            .Set("EndTime", "EndTime", request.EndTime)
            .Where("Id = @Id", new { Id = id })
            .Build();

        await db.ExecuteAsync(sql, pars);
        await activityLog.Log(userId, id, EntityType.Task, "updated");

        return await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id", new { Id = id });
    }

    public async Task<bool> SoftDelete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Task with id {id} not found.");

        await db.ExecuteAsync("UPDATE Tasks SET deleted_at = NOW() WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Task, "deleted");
        return true;
    }

    public async Task<bool> Delete(int id, int userId)
    {
        using var db = _context.CreateConnection();
        var existing = await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId", new { Id = id, UserId = userId });
        if (existing is null)
            throw new ArgumentException($"Task with id {id} not found.");

        // Delete subTasks first
        await db.ExecuteAsync("DELETE FROM SubTasks WHERE TaskId = @TaskId", new { TaskId = id });
        await db.ExecuteAsync("DELETE FROM Tasks WHERE Id = @Id", new { Id = id });
        await activityLog.Log(userId, id, EntityType.Task, "deleted_hard");
        return true;
    }

    // --- SubTask ---

    public async Task<Models.SubTask> AddSubTask(int taskId, CreateSubTaskRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var task = await db.QueryFirstOrDefaultAsync<Models.Task>(
            "SELECT * FROM Tasks WHERE Id = @Id AND UserId = @UserId AND deleted_at IS NULL",
            new { Id = taskId, UserId = userId });
        if (task is null)
            throw new ArgumentException($"Task with id {taskId} not found.");
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("SubTask title cannot be null or empty.");

        var id = await db.ExecuteScalarAsync<int>(
            @"INSERT INTO SubTasks (TaskId, Title, Status, created_at, updated_at)
              VALUES (@TaskId, @Title, @Status, NOW(), NOW());
              SELECT LAST_INSERT_ID();",
            new { TaskId = taskId, request.Title, Status = (int)request.Status });

        var subTask = (await db.QueryFirstOrDefaultAsync<Models.SubTask>(
            "SELECT * FROM SubTasks WHERE Id = @Id", new { Id = id }))!;

        await activityLog.Log(userId, subTask.Id, EntityType.SubTask, "created");

        return subTask;
    }

    public async Task<Models.SubTask?> UpdateSubTask(int subTaskId, UpdateSubTaskRequest request, int userId)
    {
        using var db = _context.CreateConnection();

        var subTask = await db.QueryFirstOrDefaultAsync<Models.SubTask>(
            @"SELECT s.* FROM SubTasks s
              INNER JOIN Tasks t ON t.Id = s.TaskId
              WHERE s.Id = @Id AND s.deleted_at IS NULL AND t.UserId = @UserId",
            new { Id = subTaskId, UserId = userId });

        if (subTask is null)
            throw new ArgumentException($"SubTask with id {subTaskId} not found.");

        var sets = new List<string>();
        var pars = new DynamicParameters();
        pars.Add("Id", subTaskId);

        if (!string.IsNullOrWhiteSpace(request.Title)) { sets.Add("Title = @Title"); pars.Add("Title", request.Title); }
        if (request.Status.HasValue) { sets.Add("Status = @Status"); pars.Add("Status", (int)request.Status.Value); }

        if (sets.Count > 0)
        {
            sets.Add("updated_at = NOW()");
            await db.ExecuteAsync($"UPDATE SubTasks SET {string.Join(", ", sets)} WHERE Id = @Id", pars);
        }

        await activityLog.Log(userId, subTaskId, EntityType.SubTask, "updated");

        return await db.QueryFirstOrDefaultAsync<Models.SubTask>(
            "SELECT * FROM SubTasks WHERE Id = @Id", new { Id = subTaskId });
    }

    public async Task<bool> DeleteSubTask(int subTaskId, int userId)
    {
        using var db = _context.CreateConnection();

        var subTask = await db.QueryFirstOrDefaultAsync<Models.SubTask>(
            @"SELECT s.* FROM SubTasks s
              INNER JOIN Tasks t ON t.Id = s.TaskId
              WHERE s.Id = @Id AND s.deleted_at IS NULL AND t.UserId = @UserId",
            new { Id = subTaskId, UserId = userId });

        if (subTask is null)
            throw new ArgumentException($"SubTask with id {subTaskId} not found.");

        await db.ExecuteAsync("UPDATE SubTasks SET deleted_at = NOW() WHERE Id = @Id", new { Id = subTaskId });
        await activityLog.Log(userId, subTaskId, EntityType.SubTask, "deleted");
        return true;
    }
}
