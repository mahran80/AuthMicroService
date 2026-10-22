namespace Auth.Application.Shared;

public sealed class LoginUserDto
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
}