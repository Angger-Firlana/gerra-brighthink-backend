using backend.DTOs.Api;
using backend.DTOs.Task;
using backend.Wrapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Task;

[ApiController]
[Route("api/task")]
public class TaskController : ControllerBase
{
    private readonly ITaskService taskService;

    public TaskController(ITaskService taskService)
    {
        this.taskService = taskService;
    }

    private int UserId => (int)HttpContext.Items["UserId"]!;

    // Index tasks with filtering + pagination
    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] TaskFilteringRequest filter)
    {
        var result = await taskService.Index(filter, UserId);

        var response = new APIResponse<IEnumerable<Models.Task>>
        {
            success = true,
            message = "Tasks retrieved successfully.",
            data = result.Items,
            pagination = result.pagination
        };

        return Ok(response);
    }

    // Get task by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await taskService.GetById(id, UserId);
        if (task is null)
            return NotFound(new APIResponse<Models.Task>
            {
                success = false,
                message = $"Task with id {id} not found."
            });

        return Ok(new APIResponse<Models.Task>
        {
            success = true,
            message = "Task retrieved successfully.",
            data = task
        });
    }

    // Create task (optional subTasks)
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var task = await taskService.Create(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
    }

    // Update task
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        var task = await taskService.Update(id, request, UserId);
        return Ok(new APIResponse<Models.Task>
        {
            success = true,
            message = "Task updated successfully.",
            data = task
        });
    }

    // Soft delete task
    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var success = await taskService.SoftDelete(id, UserId);
        return Ok(new APIResponse<Models.Task>
        {
            success = success,
            message = "Task deleted successfully."
        });
    }

    // Hard delete task
    [HttpDelete("{id}/hard")]
    public async Task<IActionResult> Delete(int id)
    {
        await taskService.Delete(id, UserId);
        return Ok(new APIResponse<Models.Task>
        {
            success = true,
            message = "Task permanently deleted."
        });
    }

    // --- SubTask endpoints ---

    [HttpPost("{taskId}/subtask")]
    public async Task<IActionResult> AddSubTask(int taskId, [FromBody] CreateSubTaskRequest request)
    {
        var subTask = await taskService.AddSubTask(taskId, request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = taskId }, subTask);
    }

    [HttpPatch("subtask/{subTaskId}")]
    public async Task<IActionResult> UpdateSubTask(int subTaskId, [FromBody] UpdateSubTaskRequest request)
    {
        var subTask = await taskService.UpdateSubTask(subTaskId, request, UserId);
        return Ok(new APIResponse<Models.SubTask>
        {
            success = true,
            message = "SubTask updated successfully.",
            data = subTask
        });
    }

    [HttpDelete("subtask/{subTaskId}")]
    public async Task<IActionResult> DeleteSubTask(int subTaskId)
    {
        await taskService.DeleteSubTask(subTaskId, UserId);
        return Ok(new APIResponse<Models.SubTask>
        {
            success = true,
            message = "SubTask deleted successfully."
        });
    }
}
