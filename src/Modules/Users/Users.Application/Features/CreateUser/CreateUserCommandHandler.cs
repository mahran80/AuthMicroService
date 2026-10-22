using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;
using Users.Application.Abstractions.Ports;
using Users.Application.DTOs;
using Users.Domain.Entities;
using Users.Domain.Errors;

namespace Users.Application.Features.CreateUser;

public sealed class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserProfileResponse>
{
    private readonly IUserProfileRepository _repository;
    private readonly IUserIdentityPort _userIdentityPort;

    public CreateUserCommandHandler(
        IUserProfileRepository repository,
        IUserIdentityPort userIdentityPort)
    {
        _repository = repository;
        _userIdentityPort = userIdentityPort;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        if (await _repository.ExistsByEmailAsync(normalizedEmail, null, cancellationToken))
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.EmailAlreadyExists);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber) &&
            await _repository.ExistsByPhoneAsync(request.CountryCode, request.PhoneNumber, null, cancellationToken))
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.PhoneAlreadyExists);
        }

        string[] nameParts = request.FullName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string firstName = nameParts.Length > 0 ? nameParts[0] : request.FullName.Trim();
        string lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

        var provisionResult = await _userIdentityPort.ProvisionUserWithoutPasswordAsync(
            normalizedEmail,
            firstName,
            lastName,
            request.CountryCode,
            request.PhoneNumber,
            cancellationToken);

        if (provisionResult.IsFailure)
        {
            return Result.Failure<UserProfileResponse>(provisionResult.Error);
        }

        Guid userId = provisionResult.Value;

        var profile = UserProfile.Create(
            userId,
            normalizedEmail,
            request.FullName,
            request.CountryCode,
            request.PhoneNumber);

        await _repository.AddAsync(profile, cancellationToken);
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
