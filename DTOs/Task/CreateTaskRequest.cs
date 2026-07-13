using backend.Enum;

namespace backend.DTOs.Task;

public class CreateTaskRequest
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TodoStatus Status { get; set; } = TodoStatus.NotStarted;
    public string Priority { get; set; } = "medium";
    public string? TypeHabbit { get; set; }
    public int? GoalId { get; set; }
    public int? TaskCategoryId { get; set; }
    public List<CreateSubTaskRequest>? SubTasks { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TodoStatus? Status { get; set; }
    public string? Priority { get; set; }
    public string? TypeHabbit { get; set; }
    public int? GoalId { get; set; }
    public int? TaskCategoryId { get; set; }
}

public class CreateSubTaskRequest
{
    public required string Title { get; set; }
    public SubTaskStatus Status { get; set; } = SubTaskStatus.NotStarted;
}

public class UpdateSubTaskRequest
{
    public string? Title { get; set; }
    public SubTaskStatus? Status { get; set; }
}
