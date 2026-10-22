namespace Auth.Application.DTOs.Logout.Response;

public sealed class LogoutResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}