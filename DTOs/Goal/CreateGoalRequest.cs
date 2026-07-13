using backend.Enum;

namespace backend.DTOs.Goal;

public class CreateGoalRequest
{
    public required string Title { get; set; }
    public string TypeGoal { get; set; } = "personal";
    public GoalStatus Status { get; set; } = GoalStatus.NotStarted;
}

public class UpdateGoalRequest
{
    public string? Title { get; set; }
    public string? TypeGoal { get; set; }
    public GoalStatus? Status { get; set; }
}

public class GoalFilteringRequest
{
    public string? search { get; set; }
    public string? status { get; set; }
    public string? typeGoal { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
