using backend.DTOs.ActivityLog;
using backend.DTOs.Api;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.ActivityLog;

[ApiController]
[Route("api/activity-log")]
public class ActivityLogController : ControllerBase
{
    private readonly IActivityLogService service;

    public ActivityLogController(IActivityLogService service)
    {
        this.service = service;
    }

    private int UserId => (int)HttpContext.Items["UserId"]!;

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] ActivityLogFilteringRequest filter)
    {
        var result = await service.Index(filter, UserId);

        return Ok(new APIResponse<IEnumerable<Models.ActivityLog>>
        {
            success = true,
            message = "Activity logs retrieved successfully.",
            data = result.Items,
            pagination = result.pagination
        });
    }
}
