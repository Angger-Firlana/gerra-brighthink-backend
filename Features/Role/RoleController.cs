using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Role;

[ApiController]
[Route("api/role")]
public class RoleController : ControllerBase
{
    private readonly IRoleService roleService;
    public RoleController(IRoleService roleService) => this.roleService = roleService;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var roles = await roleService.GetAll();
        return Ok(new { success = true, message = "Roles retrieved successfully.", data = roles });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var role = await roleService.GetById(id);
        if (role is null)
            return NotFound(new { success = false, message = $"Role with id {id} not found." });
        return Ok(new { success = true, message = "Role retrieved successfully.", data = role });
    }
}
