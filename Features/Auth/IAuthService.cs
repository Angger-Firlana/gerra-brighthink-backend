using backend.DTOs.Auth;

namespace backend.Features.Auth;

public interface IAuthService
{
    Task<LoginResponse> Login(string identity, string password);
    Task<Models.User?> GetMe(int userId);
}
