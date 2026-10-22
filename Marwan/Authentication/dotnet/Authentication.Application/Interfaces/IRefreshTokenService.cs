using Authentication.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Authentication.Application.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<(string Token, DateTimeOffset ExpiresAt)> CreateAsync(ApplicationUser user);
        Task<RefreshToken?> ValidateAsync(string token);
        Task<(string Token, DateTimeOffset ExpiresAt)> RotateAsync(RefreshToken refreshToken);
        Task RevokeAsync(RefreshToken refreshToken);
        Task RevokeAllAsync(Guid userId);
    }
}
