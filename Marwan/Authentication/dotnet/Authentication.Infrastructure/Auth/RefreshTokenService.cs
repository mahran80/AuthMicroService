using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Auth
{
    public class RefreshTokenService(
        IamDbContext _dbContext,
        IOptions<JwtSettings> jwtOptions)
        : IRefreshTokenService
    {
        private readonly JwtSettings _jwtSettings = jwtOptions.Value;

        public async Task<(string Token, DateTimeOffset ExpiresAt)> CreateAsync(ApplicationUser user)
        {
            var (token, tokenHash, expiresAt) = GenerateToken();

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TokenHash = tokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt.UtcDateTime
            };

            _dbContext.RefreshTokens.Add(refreshToken);

            await _dbContext.SaveChangesAsync();

            return (token, expiresAt);
        }

        public async Task<RefreshToken?> ValidateAsync(string token)        
        {
            var tokenHash = HashToken(token);

            var refreshToken = await _dbContext.RefreshTokens
                    .Include(x => x.User)
                    .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (refreshToken is null ||
                refreshToken.RevokedAt.HasValue ||
                refreshToken.ExpiresAt <= DateTime.UtcNow ||
                refreshToken.User.Status != UserStatus.Active)
            {
                return null;
            }

            return refreshToken;
        }

        public async Task<(string Token, DateTimeOffset ExpiresAt)> RotateAsync(RefreshToken refreshToken)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;

            var (newToken, newTokenHash, expiresAt) = GenerateToken();

            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = refreshToken.UserId,
                TokenHash = newTokenHash,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt.UtcDateTime
            };

            refreshToken.ReplacedByTokenId = newRefreshToken.Id;

            _dbContext.RefreshTokens.Add(newRefreshToken);

            await _dbContext.SaveChangesAsync();

            return (newToken, expiresAt);
        }

        public async Task RevokeAsync(RefreshToken refreshToken)
        {
            if (refreshToken.RevokedAt.HasValue)
                return;

            refreshToken.RevokedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public async Task RevokeAllAsync(Guid userId)
        {
            var refreshTokens = await _dbContext.RefreshTokens
                    .Where(x =>
                        x.UserId == userId &&
                        !x.RevokedAt.HasValue &&
                        x.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync();

            if (refreshTokens.Count == 0)
                return;

            foreach (var refreshToken in refreshTokens)
            {
                refreshToken.RevokedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        //Helpers
        private (string Token, string TokenHash, DateTimeOffset ExpiresAt) GenerateToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);

            var token = Convert.ToBase64String(tokenBytes);

            var tokenHash = HashToken(token);

            var expiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            return (token, tokenHash, expiresAt);
        }

        private static string HashToken(string token)
        {
            return Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }
}