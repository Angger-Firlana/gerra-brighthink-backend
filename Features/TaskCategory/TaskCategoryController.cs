using backend.DTOs.Api;
using backend.DTOs.TaskCategory;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.TaskCategory;

[ApiController]
[Route("api/task-category")]
public class TaskCategoryController : ControllerBase
{
    private readonly ITaskCategoryService service;

    public TaskCategoryController(ITaskCategoryService service)
    {
        this.service = service;
    }

    private int UserId => (int)HttpContext.Items["UserId"]!;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var categories = await service.Index(UserId);
        var response = new APIResponse<IEnumerable<Models.TaskCategory>>
        {
            success = true,
            message = "Categories retrieved successfully.",
            data = categories
        };
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await service.GetById(id, UserId);
        if (category is null)
            return NotFound(new APIResponse<Models.TaskCategory>
            {
                success = false,
                message = $"Category with id {id} not found."
            });

        return Ok(new APIResponse<Models.TaskCategory>
        {
            success = true,
            message = "Category retrieved successfully.",
            data = category
        });
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateTaskCategoryRequest request)
    {
        var category = await service.Create(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskCategoryRequest request)
    {
        var category = await service.Update(id, request, UserId);
        return Ok(new APIResponse<Models.TaskCategory>
        {
            success = true,
            message = "Category updated successfully.",
            data = category
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.Delete(id, UserId);
        return Ok(new APIResponse<Models.TaskCategory>
        {
            success = true,
            message = "Category deleted successfully."
        });
    }
}
