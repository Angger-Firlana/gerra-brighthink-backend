namespace backend.DTOs.Auth;

public class LoginRequest
{
    public required string Identity {get; set;}
    public required string Password {get; set;}
}
