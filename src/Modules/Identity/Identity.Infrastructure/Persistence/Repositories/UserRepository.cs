using Identity.Application.Abstractions.Ports;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token, ct);
    }

    public async Task AddUserAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, ct);
    }

    public async Task<int> RevokeAllActiveUserTokensAsync(Guid userId, string reason, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAtUtc == null && r.ExpiresAtUtc > now)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
        }

        return activeTokens.Count;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.CountryCode == countryCode && u.PhoneNumber == phoneNumber, ct);
    }

    public async Task<bool> ExistsByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default)
    {
        return await _context.Users
            .AnyAsync(u => u.CountryCode == countryCode && u.PhoneNumber == phoneNumber, ct);
    }

    public async Task AddOtpVerificationAsync(OtpVerification otp, CancellationToken ct = default)
    {
        await _context.OtpVerifications.AddAsync(otp, ct);
    }

    public async Task<OtpVerification?> GetLatestActiveOtpAsync(Guid userId, Identity.Domain.Enums.OtpPurpose purpose, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.OtpVerifications
            .Where(o => o.UserId == userId && o.Purpose == purpose && o.UsedAtUtc == null && o.ExpiresAtUtc > now && o.FailedAttempts < 5)
            .OrderByDescending(o => o.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken ct = default)
    {
        await _context.PasswordResetTokens.AddAsync(token, ct);
    }

    public async Task<PasswordResetToken?> GetActivePasswordResetTokenByHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _context.PasswordResetTokens
            .FirstOrDefaultAsync(p => p.TokenHash == tokenHash && p.UsedAtUtc == null && p.ExpiresAtUtc > now, ct);
    }

    public async Task AddLoginSessionAsync(LoginSession session, CancellationToken ct = default)
    {
        await _context.LoginSessions.AddAsync(session, ct);
    }

    public async Task TerminateLoginSessionByRefreshTokenAsync(Guid refreshTokenId, CancellationToken ct = default)
    {
        var sessions = await _context.LoginSessions
            .Where(s => s.RefreshTokenId == refreshTokenId && s.IsActive)
            .ToListAsync(ct);

        foreach (var session in sessions)
        {
            session.TerminateSession();
        }
    }

    public async Task<User?> GetByExternalLoginAsync(string provider, string providerKey, CancellationToken ct = default)
    {
        var externalLogin = await _context.ExternalLogins
            .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderKey == providerKey, ct);

        if (externalLogin is null)
        {
            return null;
        }

        return await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == externalLogin.UserId, ct);
    }

    public async Task<bool> ExternalLoginExistsAsync(Guid userId, string provider, CancellationToken ct = default)
    {
        return await _context.ExternalLogins
            .AnyAsync(e => e.UserId == userId && e.Provider == provider, ct);
    }

    public async Task AddExternalLoginAsync(ExternalLogin externalLogin, CancellationToken ct = default)
    {
        await _context.ExternalLogins.AddAsync(externalLogin, ct);
    }
}
