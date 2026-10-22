namespace Auth.Application.DTOs.LoginWeb.Response;


public sealed class LoginWebResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }
    public bool RememberMe { get; set; }
}