using SharedKernel.Application;

namespace SharedKernel.Contracts;

public interface IUserIdentityPort
{
    Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<int>> UpdateUserStatusAndRevokeTokensAsync(Guid userId, string newStatus, CancellationToken cancellationToken = default);

    Task<Result<Guid>> ProvisionUserWithoutPasswordAsync(
        string email,
        string firstName,
        string lastName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default);

    Task<Result> UpdateUserContactInIdentityAsync(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default);
}
