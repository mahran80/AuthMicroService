namespace Auth.Application.DTOs.RefreshToken.Response;

public sealed class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;

     public string RefreshToken { get; set; } = string.Empty;

    // public DateTime ExpiredAt { get; set; }
}