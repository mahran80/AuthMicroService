using Identity.Application.Abstractions.Ports;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Contracts;

namespace Identity.Infrastructure.Integration;

public sealed class UserIdentityAdapter : IUserIdentityPort
{
    private readonly IUserRepository _userRepository;

    public UserIdentityAdapter(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is not null;
    }

    public async Task<Result<int>> UpdateUserStatusAndRevokeTokensAsync(
        Guid userId,
        string newStatus,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<int>(IdentityErrors.UserNotFound);
        }

        if (!Enum.TryParse<UserStatus>(newStatus, ignoreCase: true, out var targetStatus))
        {
            return Result.Failure<int>(Error.Validation(
                "Identity.InvalidStatus",
                $"Status '{newStatus}' is not a valid user status."));
        }

        switch (targetStatus)
        {
            case UserStatus.Active:
                user.Activate();
                break;
            case UserStatus.Suspended:
                user.Suspend();
                break;
            case UserStatus.Deactivated:
                user.Deactivate();
                break;
        }

        int revokedTokensCount = 0;
        if (targetStatus == UserStatus.Suspended || targetStatus == UserStatus.Deactivated)
        {
            string auditReason = targetStatus == UserStatus.Suspended
                ? "AdminSuspended"
                : "AdminDeactivated";

            revokedTokensCount = await _userRepository.RevokeAllActiveUserTokensAsync(
                userId,
                auditReason,
                cancellationToken);
        }

        await _userRepository.SaveChangesAsync(cancellationToken);
        return Result.Success(revokedTokensCount);
    }
}
