using Microsoft.EntityFrameworkCore;
using Users.Application.Abstractions.Ports;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository
{
    private readonly UsersDbContext _dbContext;

    public UserProfileRepository(UsersDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<UserProfile?> GetByPhoneAsync(string? countryCode, string phoneNumber, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.CountryCode == countryCode && u.PhoneNumber == phoneNumber && !u.IsDeleted, cancellationToken);
    }

    public async Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        return await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<UserProfile>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _dbContext.UserProfiles.Where(u => u.Email == normalizedEmail && !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByPhoneAsync(string? countryCode, string phoneNumber, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserProfiles.Where(u =>
            u.CountryCode == countryCode &&
            u.PhoneNumber == phoneNumber &&
            !u.IsDeleted);

        if (excludeUserId.HasValue)
        {
            query = query.Where(u => u.Id != excludeUserId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserProfiles.AddAsync(profile, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
