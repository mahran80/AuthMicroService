namespace Auth.Application.DTOs.Register.Response;

public sealed class RegisterResponse
{
    // public Guid UserId { get; set; }

    public string Message { get; set; } = string.Empty;


    public string OtpCode { get; set; } = default!;
}