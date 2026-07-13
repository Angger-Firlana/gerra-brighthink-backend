using backend.DTOs.Task;
using backend.Wrapper;

namespace backend.Features.Task;

public interface ITaskService
{
    Task<PagedResult<Models.Task>> Index(TaskFilteringRequest filter, int userId);
    Task<Models.Task?> GetById(int id, int userId);
    Task<Models.Task> Create(CreateTaskRequest request, int userId);
    Task<Models.Task?> Update(int id, UpdateTaskRequest request, int userId);
    Task<bool> SoftDelete(int id, int userId);
    Task<bool> Delete(int id, int userId);
    // SubTask
    Task<Models.SubTask> AddSubTask(int taskId, CreateSubTaskRequest request, int userId);
    Task<Models.SubTask?> UpdateSubTask(int subTaskId, UpdateSubTaskRequest request, int userId);
    Task<bool> DeleteSubTask(int subTaskId, int userId);
}
