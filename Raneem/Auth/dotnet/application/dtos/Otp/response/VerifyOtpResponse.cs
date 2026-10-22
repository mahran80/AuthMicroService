namespace Auth.Application.DTOs.Otp.Response;

public sealed class VerifyOtpResponse
{
    public bool Success { get; set; }
public Guid? DriverId { get; set; }

public Guid? PassengerId { get; set; }
    public string Message { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Name { get; set; } = default!;
    public string? AccessToken { get; set; }

    public DateTime? AccessTokenExpiresAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }
}