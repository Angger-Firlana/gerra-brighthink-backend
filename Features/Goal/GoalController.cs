using backend.DTOs.Api;
using backend.DTOs.Goal;
using backend.Wrapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Goal;

[ApiController]
[Route("api/goal")]
public class GoalController : ControllerBase
{
    private readonly IGoalService goalService;

    public GoalController(IGoalService goalService)
    {
        this.goalService = goalService;
    }

    private int UserId => (int)HttpContext.Items["UserId"]!;

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] GoalFilteringRequest filter)
    {
        var result = await goalService.Index(filter, UserId);

        return Ok(new APIResponse<IEnumerable<Models.Goal>>
        {
            success = true,
            message = "Goals retrieved successfully.",
            data = result.Items,
            pagination = result.pagination
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var goal = await goalService.GetById(id, UserId);
        if (goal is null)
            return NotFound(new APIResponse<Models.Goal>
            {
                success = false,
                message = $"Goal with id {id} not found."
            });

        return Ok(new APIResponse<Models.Goal>
        {
            success = true,
            message = "Goal retrieved successfully.",
            data = goal
        });
    }

    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateGoalRequest request)
    {
        var goal = await goalService.Create(request, UserId);
        return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGoalRequest request)
    {
        var goal = await goalService.Update(id, request, UserId);
        return Ok(new APIResponse<Models.Goal>
        {
            success = true,
            message = "Goal updated successfully.",
            data = goal
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> SoftDelete(int id)
    {
        await goalService.SoftDelete(id, UserId);
        return Ok(new APIResponse<Models.Goal>
        {
            success = true,
            message = "Goal deleted successfully."
        });
    }

    [HttpDelete("{id}/hard")]
    public async Task<IActionResult> Delete(int id)
    {
        await goalService.Delete(id, UserId);
        return Ok(new APIResponse<Models.Goal>
        {
            success = true,
            message = "Goal permanently deleted."
        });
    }
}
