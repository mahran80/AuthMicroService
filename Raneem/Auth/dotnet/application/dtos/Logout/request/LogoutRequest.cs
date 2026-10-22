namespace Auth.Application.DTOs.Logout.Request;

public sealed class LogoutRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}