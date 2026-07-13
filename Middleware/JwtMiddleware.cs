using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace backend.Middleware;

public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;

    public JwtMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        // Bypass: [SkipAuth], non-MVC routes (Swagger, static files), or no endpoint yet
        if (endpoint is null || endpoint.Metadata.GetMetadata<SkipAuthAttribute>() is not null)
        {
            await _next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Token Required\"}");
            return;
        }

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Invalid Token\"}");
            return;
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            context.User = principal;

            // Convenience items for controllers
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usernameClaim = principal.FindFirst(ClaimTypes.Name)?.Value;
            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim is not null)
                context.Items["UserId"] = int.Parse(userIdClaim);
            if (usernameClaim is not null)
                context.Items["Username"] = usernameClaim;
            if (roleClaim is not null)
                context.Items["Role"] = roleClaim;

            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Token Expired\"}");
        }
        catch (SecurityTokenException)
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"success\":false,\"message\":\"Invalid Token\"}");
        }
    }
}
