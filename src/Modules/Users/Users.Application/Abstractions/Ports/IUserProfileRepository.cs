using Users.Domain.Entities;

namespace Users.Application.Abstractions.Ports;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByPhoneAsync(string? countryCode, string phoneNumber, CancellationToken cancellationToken = default);
    Task<UserProfile?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserProfile>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneAsync(string? countryCode, string phoneNumber, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    Task AddAsync(UserProfile profile, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
