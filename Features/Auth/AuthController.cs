using backend.DTOs.Auth;
using backend.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace backend.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    readonly IAuthService authService;
    public AuthController(IAuthService authService)
    {
        this.authService = authService;
    }

    [HttpPost("login")]
    [SkipAuth]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await authService.Login(request.Identity, request.Password);
        if (response.code == 404)
            return NotFound(response);

        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = (int)HttpContext.Items["UserId"]!;
        var user = await authService.GetMe(userId);
        return Ok(user);
    }
}
