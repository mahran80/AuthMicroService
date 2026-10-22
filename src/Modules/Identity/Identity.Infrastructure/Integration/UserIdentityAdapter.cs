using Identity.Application.Abstractions.Ports;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Contracts;

namespace Identity.Infrastructure.Integration;

public sealed class UserIdentityAdapter : IUserIdentityPort
{
    private readonly IUserRepository _userRepository;
    private readonly IRbacAccessPort _rbacAccessPort;
    private readonly IPasswordHasherPort _passwordHasher;

    public UserIdentityAdapter(
        IUserRepository userRepository,
        IRbacAccessPort rbacAccessPort,
        IPasswordHasherPort passwordHasher)
    {
        _userRepository = userRepository;
        _rbacAccessPort = rbacAccessPort;
        _passwordHasher = passwordHasher;
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

    public async Task<Result<Guid>> ProvisionUserWithoutPasswordAsync(
        string email,
        string firstName,
        string lastName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            return Result.Failure<Guid>(IdentityErrors.EmailAlreadyInUse);
        }

        // Random unguessable password hash for invite flow
        string randomPassword = Guid.NewGuid().ToString("N") + "!" + Guid.NewGuid().ToString("N");
        string placeholderPasswordHash = _passwordHasher.HashPassword(randomPassword);

        var user = User.Create(
            normalizedEmail,
            placeholderPasswordHash,
            firstName,
            lastName);

        await _userRepository.AddUserAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        const string defaultRole = "StandardUser";
        var rbacResult = await _rbacAccessPort.AssignDefaultRoleAsync(user.Id, defaultRole, cancellationToken);
        if (rbacResult.IsFailure)
        {
            return Result.Failure<Guid>(rbacResult.Error);
        }

        return Result.Success(user.Id);
    }

    public async Task<Result> UpdateUserContactInIdentityAsync(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(IdentityErrors.UserNotFound);
        }

        string normalizedEmail = email.Trim().ToLowerInvariant();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
            {
                return Result.Failure(IdentityErrors.EmailAlreadyInUse);
            }
        }

        user.UpdateContact(normalizedEmail, firstName, lastName);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
