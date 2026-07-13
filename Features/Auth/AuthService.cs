using backend.Data;
using backend.DTOs.Auth;
using backend.Helpers;
using Microsoft.EntityFrameworkCore;

namespace backend.Features.Auth;

public class AuthService : IAuthService
{
    private readonly AppDbContext dbContext;
    private readonly GenerateToken generateToken;

    public AuthService(AppDbContext dbContext, GenerateToken generateToken)
    {
        this.dbContext = dbContext;
        this.generateToken = generateToken;
    }

    public async Task<LoginResponse> Login(string identity, string password)
    {
        var response = new LoginResponse();

        var user = await dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(x => x.Email == identity || x.Username == identity);

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
        return await dbContext.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
}
