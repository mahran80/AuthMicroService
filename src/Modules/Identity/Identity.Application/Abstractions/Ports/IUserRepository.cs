using Identity.Domain.Entities;

namespace Identity.Application.Abstractions.Ports;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);
    Task AddUserAsync(User user, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);
    Task<int> RevokeAllActiveUserTokensAsync(Guid userId, string reason, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
