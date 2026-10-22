namespace Auth.Application.DTOs.RefreshToken.Request;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}