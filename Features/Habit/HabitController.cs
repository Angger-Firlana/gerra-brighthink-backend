using backend.DTOs.Api;
using backend.DTOs.Habit;
using backend.Wrapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Habit;

[ApiController]
[Route("api/habit")]
public class HabitController : ControllerBase
{
    private readonly IHabitService habitService;

    public HabitController(IHabitService habitService)
    {
        this.habitService = habitService;
    }

    private int UserId => (int)HttpContext.Items["UserId"]!;

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] HabitFilteringRequest filter)
    {
        var result = await habitService.Index(filter, UserId);

        return Ok(new APIResponse<IEnumerable<Models.Habit>>
        {
            success = true,
            message = "Habits retrieved successfully.",
            data = result.Items,
            pagination = result.pagination
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var habit = await habitService.GetById(id, UserId);
        if (habit is null)
            return NotFound(new APIResponse<Models.Habit>
            {
                success = false,
                message = $"Habit with id {id} not found."
            });

        return Ok(new APIResponse<Models.Habit>
        {
            success = true,
            message = "Habit retrieved successfully.",
            data = habit
        });
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateHabitRequest request)
    {
        var habit = await habitService.Create(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = habit.Id }, habit);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateHabitRequest request)
    {
        var habit = await habitService.Update(id, request, UserId);
        return Ok(new APIResponse<Models.Habit>
        {
            success = true,
            message = "Habit updated successfully.",
            data = habit
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        await habitService.SoftDelete(id, UserId);
        return Ok(new APIResponse<Models.Habit>
        {
            success = true,
            message = "Habit deleted successfully."
        });
    }

    [HttpDelete("{id}/hard")]
    public async Task<IActionResult> Delete(int id)
    {
        await habitService.Delete(id, UserId);
        return Ok(new APIResponse<Models.Habit>
        {
            success = true,
            message = "Habit permanently deleted."
        });
    }
}
