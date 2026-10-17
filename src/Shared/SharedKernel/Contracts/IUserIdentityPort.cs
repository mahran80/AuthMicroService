using SharedKernel.Application;

namespace SharedKernel.Contracts;

public interface IUserIdentityPort
{
    Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<int>> UpdateUserStatusAndRevokeTokensAsync(Guid userId, string newStatus, CancellationToken cancellationToken = default);
}
