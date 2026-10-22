using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Auth.Application.Ports.Outbound;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Infrastructure.Services.Security;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(Guid userId)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256);

        var accessTokenMinutes =
            int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString("N") +
               Guid.NewGuid().ToString("N");
    }

    public DateTime GetAccessTokenExpiration()
    {
        var accessTokenMinutes =
            int.Parse(_configuration["Jwt:AccessTokenMinutes"]!);

        return DateTime.UtcNow.AddMinutes(accessTokenMinutes);
    }

 public DateTime GetRefreshTokenExpiration()
{
    var refreshTokenDays =
        int.Parse(_configuration["Jwt:RefreshTokenMinutes"]!);

    return DateTime.UtcNow.AddDays(refreshTokenDays);
}
}