namespace Auth.Application.DTOs.Otp.Response;

public sealed class ResendOtpResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
    
    public string? OtpCode { get; set; }

}