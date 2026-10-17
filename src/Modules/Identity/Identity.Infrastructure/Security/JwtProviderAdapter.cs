using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Security;

public sealed class JwtProviderAdapter : IJwtProviderPort
{
    private readonly JwtOptions _jwtOptions;

    public JwtProviderAdapter(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public TokenPairResult GenerateTokenPair(
        User user,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions)
    {
        var jwtId = Guid.NewGuid().ToString("D");
        var now = DateTime.UtcNow;
        var accessTokenExpiresAtUtc = now.AddMinutes(_jwtOptions.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAtUtc = now.AddDays(_jwtOptions.RefreshTokenExpirationDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name, $"{user.FirstName} {user.LastName}".Trim()),
            new(JwtRegisteredClaimNames.Jti, jwtId)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim("roles", role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permissions", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = accessTokenExpiresAtUtc,
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        var refreshTokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(refreshTokenBytes);
        var refreshToken = Convert.ToBase64String(refreshTokenBytes);

        return new TokenPairResult(
            accessToken,
            refreshToken,
            jwtId,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc);
    }
}
