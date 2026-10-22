namespace Auth.Application.DTOs.VerifyLoginOtp.Response;

public sealed class VerifyLoginOtpResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAt { get; set; }
}