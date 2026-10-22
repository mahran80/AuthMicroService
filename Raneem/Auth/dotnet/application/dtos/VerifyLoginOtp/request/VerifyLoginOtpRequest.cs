namespace Auth.Application.DTOs.VerifyLoginOtp.Request;
public sealed class VerifyLoginOtpRequest
{
    public string? Email { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }
    //  public Guid UserId { get; set; }

    public string Code { get; set; } = string.Empty;
}