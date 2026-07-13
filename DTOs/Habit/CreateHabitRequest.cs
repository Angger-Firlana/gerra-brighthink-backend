using backend.Enum;

namespace backend.DTOs.Habit;

public class CreateHabitRequest
{
    public required string Title { get; set; }
    public string Period { get; set; } = "daily";
    public int TargetMinutes { get; set; }
    public HabitStatus Status { get; set; } = HabitStatus.NotStarted;
}

public class UpdateHabitRequest
{
    public string? Title { get; set; }
    public string? Period { get; set; }
    public int? TargetMinutes { get; set; }
    public int? CurrentMinutes { get; set; }
    public HabitStatus? Status { get; set; }
}

public class HabitFilteringRequest
{
    public string? search { get; set; }
    public string? status { get; set; }
    public string? period { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
