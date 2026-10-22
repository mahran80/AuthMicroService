namespace Auth.Application.DTOs.Login.Response;

public sealed class LoginUser
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}