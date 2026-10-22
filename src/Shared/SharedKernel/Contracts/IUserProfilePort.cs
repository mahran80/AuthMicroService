using SharedKernel.Application;

namespace SharedKernel.Contracts;

public interface IUserProfilePort
{
    Task<Result> CreateOrSyncProfileAsync(
        Guid userId,
        string email,
        string fullName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default);

    Task<Result> SoftDeleteProfileAsync(Guid userId, CancellationToken cancellationToken = default);
}
