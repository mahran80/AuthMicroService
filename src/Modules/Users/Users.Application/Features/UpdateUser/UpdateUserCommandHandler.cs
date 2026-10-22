using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;
using Users.Application.Abstractions.Ports;
using Users.Application.DTOs;
using Users.Domain.Errors;

namespace Users.Application.Features.UpdateUser;

public sealed class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UserProfileResponse>
{
    private readonly IUserProfileRepository _repository;
    private readonly IUserIdentityPort _userIdentityPort;

    public UpdateUserCommandHandler(
        IUserProfileRepository repository,
        IUserIdentityPort userIdentityPort)
    {
        _repository = repository;
        _userIdentityPort = userIdentityPort;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.ProfileNotFound);
        }

        if (profile.IsDeleted)
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.ProfileAlreadyDeleted);
        }

        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _repository.ExistsByEmailAsync(normalizedEmail, request.UserId, cancellationToken))
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.EmailAlreadyExists);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _repository.ExistsByPhoneAsync(request.CountryCode, request.PhoneNumber, request.UserId, cancellationToken))
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.PhoneAlreadyExists);
        }

        string[] nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string firstName = nameParts.Length > 0 ? nameParts[0] : request.FullName.Trim();
        string lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var syncResult = await _userIdentityPort.UpdateUserContactInIdentityAsync(
            request.UserId,
            normalizedEmail,
            firstName,
            lastName,
            request.CountryCode,
            request.PhoneNumber,
            cancellationToken);

        if (syncResult.IsFailure)
        {
            return Result.Failure<UserProfileResponse>(syncResult.Error);
        }

        profile.UpdateDetails(
            request.FullName,
            normalizedEmail,
            request.CountryCode,
            request.PhoneNumber);

        await _repository.SaveChangesAsync(cancellationToken);

        var response = new UserProfileResponse(
            profile.Id,
            profile.FullName,
            profile.Email,
            profile.CountryCode,
            profile.PhoneNumber,
            profile.IsDeleted,
            profile.CreatedAtUtc,
            profile.UpdatedAtUtc);

        return Result.Success(response);
    }
}
