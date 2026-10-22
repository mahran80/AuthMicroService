namespace Auth.Application.DTOs.ResendOtp.Request;

public sealed class ResendOtpRequest
{
    public string? Email { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }
}