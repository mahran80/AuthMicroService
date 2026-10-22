namespace Auth.Application.DTOs.ChangePassword.Response;

public sealed class ChangePasswordResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = null!;
}