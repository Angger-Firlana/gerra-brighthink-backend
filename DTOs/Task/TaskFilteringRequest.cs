namespace backend.DTOs.Task;

public class TaskFilteringRequest
{
    public string? search { get; set; }
    public string? status { get; set; }
    public string? priority { get; set; }
    public int goalId { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}
