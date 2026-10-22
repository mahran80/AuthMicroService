using Authentication.Domain.Entities;
using Authentication.Domain.Repositories;
using Authentication.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authentication.Infrastructure.Persistence.Repositories
{
    public class EmailVerificationOtpRepository : IEmailVerificationOtpRepository
    {
        private readonly IamDbContext _dbContext;

        public EmailVerificationOtpRepository(IamDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(EmailVerificationOtp otp, CancellationToken cancellationToken = default)
        {
            await _dbContext.EmailVerificationOtps.AddAsync(otp, cancellationToken);
        }

        public async Task<EmailVerificationOtp?> GetActiveOtpByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            return await _dbContext.EmailVerificationOtps
                .Where(x =>
                    x.UserId == userId &&
                    x.UsedAt == null &&
                    x.ExpiresAt > now)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InvalidateOtpsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var activeOtps = await _dbContext.EmailVerificationOtps
                .Where(x => x.UserId == userId && x.UsedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var otp in activeOtps)
            {
                otp.UsedAt = now;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
