namespace Auth.Application.Ports.Outbound.web;

public interface IJwtServiceWeb
{
    string GenerateAccessToken(Guid userId);

    string GenerateRefreshToken();

    DateTime GetAccessTokenExpiration();

DateTime GetRefreshTokenExpiration(bool rememberMe);
}