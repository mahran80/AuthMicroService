namespace Auth.Application.DTOs.Login.Response;

public sealed class LoginResponse
{
    // public Guid UserId { get; set; }

    public string Message { get; set; } = string.Empty;


    public string OtpCode { get; set; } = default!;
}