namespace backend.DTOs.TaskCategory;

public class CreateTaskCategoryRequest
{
    public required string Name { get; set; }
}

public class UpdateTaskCategoryRequest
{
    public string? Name { get; set; }
}
