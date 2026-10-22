namespace Auth.Application.DTOs.Otp.Request;

public sealed class OtpCommandRequest
{
    public string ProjectType { get; set; } = null!;

    public string CommandType { get; set; } = null!;

    public string? Email { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Code { get; set; }
}