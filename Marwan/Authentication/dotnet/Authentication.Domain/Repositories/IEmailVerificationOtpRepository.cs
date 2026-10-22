using Authentication.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Domain.Repositories
{
    public interface IEmailVerificationOtpRepository
    {
        Task AddAsync(EmailVerificationOtp otp, CancellationToken cancellationToken = default);
        Task<EmailVerificationOtp?> GetActiveOtpByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task InvalidateOtpsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
