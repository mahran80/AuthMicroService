using Identity.Domain.Entities;
using Identity.Domain.Enums;

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

    Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default);
    Task<bool> ExistsByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default);
    Task AddOtpVerificationAsync(OtpVerification otp, CancellationToken ct = default);
    Task<OtpVerification?> GetLatestActiveOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default);
    Task AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken ct = default);
    Task<PasswordResetToken?> GetActivePasswordResetTokenByHashAsync(string tokenHash, CancellationToken ct = default);
    Task AddLoginSessionAsync(LoginSession session, CancellationToken ct = default);
    Task TerminateLoginSessionByRefreshTokenAsync(Guid refreshTokenId, CancellationToken ct = default);
}
