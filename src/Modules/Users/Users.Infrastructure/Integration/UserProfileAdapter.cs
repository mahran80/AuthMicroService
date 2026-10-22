using SharedKernel.Application;
using SharedKernel.Contracts;
using Users.Application.Abstractions.Ports;
using Users.Domain.Entities;
using Users.Domain.Errors;

namespace Users.Infrastructure.Integration;

public sealed class UserProfileAdapter : IUserProfilePort
{
    private readonly IUserProfileRepository _repository;

    public UserProfileAdapter(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> CreateOrSyncProfileAsync(
        Guid userId,
        string email,
        string fullName,
        string? countryCode,
        string? phoneNumber,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            var profile = UserProfile.Create(
                userId,
                email,
                fullName,
                countryCode,
                phoneNumber);

            await _repository.AddAsync(profile, cancellationToken);
        }
        else
        {
            existing.UpdateDetails(
                fullName,
                email,
                countryCode ?? existing.CountryCode,
                phoneNumber ?? existing.PhoneNumber);

            if (existing.IsDeleted)
            {
                existing.Restore();
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> SoftDeleteProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(userId, cancellationToken);
        if (existing is null)
        {
            return Result.Failure(UsersErrors.ProfileNotFound);
        }

        if (existing.IsDeleted)
        {
            return Result.Failure(UsersErrors.ProfileAlreadyDeleted);
        }

        existing.SoftDelete();
        await _repository.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
