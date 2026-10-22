namespace Auth.Application.DTOs.ForgotPassword.Request;

public sealed class ForgotPasswordRequest
{
    public string? Email { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }
}