namespace Auth.Application.DTOs.ResetPassword.Response;

public sealed class ResetPasswordResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}