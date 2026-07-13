namespace backend.DTOs.ActivityLog;

public class ActivityLogFilteringRequest
{
    public string? action { get; set; }
    public int? entityType { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 20;
}
