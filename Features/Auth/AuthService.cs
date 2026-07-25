using Dapper;
using backend.Data;
using backend.DTOs.Auth;

namespace backend.Features.Auth;

public class AuthService : IAuthService
{
    private readonly DapperContext _context;
    private readonly Helpers.GenerateToken generateToken;

    public AuthService(DapperContext context, Helpers.GenerateToken generateToken)
    {
        _context = context;
        this.generateToken = generateToken;
    }

    public async Task<LoginResponse> Login(string identity, string password)
    {
        var response = new LoginResponse();

        using var db = _context.CreateConnection();

        var users = await db.QueryAsync<Models.User, Models.Role, Models.User>(
            @"SELECT u.*, r.Id, r.Name FROM Users u
              INNER JOIN Roles r ON r.Id = u.RoleId
              WHERE (u.Email = @Identity OR u.Username = @Identity)
                AND u.deleted_at IS NULL",
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { Identity = identity },
            splitOn: "Id");

        var user = users.FirstOrDefault();

        if (user is null)
        {
            response.code = 404;
            response.message = "username or password is wrong";
            return response;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            response.code = 404;
            response.message = "username or password is wrong";
            return response;
        }

        var token = generateToken.execute(user);

        response.code = 200;
        response.message = "login successfully";
        response.token = token;
        response.user = user;
        response.expiredAt = DateTime.UtcNow.AddDays(7);

        return response;
    }

    public async Task<Models.User?> GetMe(int userId)
    {
        using var db = _context.CreateConnection();

        var users = await db.QueryAsync<Models.User, Models.Role, Models.User>(
            @"SELECT u.*, r.Id, r.Name FROM Users u
              INNER JOIN Roles r ON r.Id = u.RoleId
              WHERE u.Id = @UserId AND u.deleted_at IS NULL",
            (user, role) =>
            {
                user.Role = role;
                return user;
            },
            new { UserId = userId },
            splitOn: "Id");

        return users.FirstOrDefault();
    }
}
