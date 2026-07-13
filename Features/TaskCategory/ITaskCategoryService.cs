namespace backend.Features.TaskCategory;

public interface ITaskCategoryService
{
    Task<IEnumerable<Models.TaskCategory>> Index(int userId);
    Task<Models.TaskCategory?> GetById(int id, int userId);
    Task<Models.TaskCategory> Create(DTOs.TaskCategory.CreateTaskCategoryRequest request, int userId);
    Task<Models.TaskCategory?> Update(int id, DTOs.TaskCategory.UpdateTaskCategoryRequest request, int userId);
    Task<bool> Delete(int id, int userId);
}
