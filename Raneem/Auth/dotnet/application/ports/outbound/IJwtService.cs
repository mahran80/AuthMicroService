namespace Auth.Application.Ports.Outbound;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId);

    string GenerateRefreshToken();

    DateTime GetAccessTokenExpiration();

    DateTime GetRefreshTokenExpiration();
}