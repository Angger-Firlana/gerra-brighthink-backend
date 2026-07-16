using backend.Enum;

namespace backend.DTOs.Goal;

public class CreateGoalRequest
{
    public required string Title { get; set; }
    public string TypeGoal { get; set; } = "personal";
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
    public DateTime? DueDate { get; set; }
}

public class UpdateGoalRequest
{
    public string? Title { get; set; }
    public string? TypeGoal { get; set; }
    public GoalStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
}

public class GoalFilteringRequest
{
    public string? search { get; set; }
    public string? status { get; set; }
    public string? typeGoal { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}

public class GoalItemDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string TypeGoal { get; set; } = string.Empty;
    public GoalStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int TaskCount { get; set; }
    public int DoneCount { get; set; }
}
