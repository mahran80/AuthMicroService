namespace Auth.Application.DTOs.ChangePassword.Request;

public sealed class ChangePasswordRequest
{
    public string Token { get; set; } = null!;

    public string NewPassword { get; set; } = null!;
}