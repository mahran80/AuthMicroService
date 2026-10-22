using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using Users.Application.Abstractions.Ports;
using Users.Application.DTOs;

namespace Users.Application.Features.GetAllUsers;

public sealed class GetAllUsersQueryHandler : IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserProfileResponse>>
{
    private readonly IUserProfileRepository _repository;

    public GetAllUsersQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<UserProfileResponse>>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = await _repository.GetAllActiveAsync(cancellationToken);

        var response = profiles
            .Select(profile => new UserProfileResponse(
                profile.Id,
                profile.FullName,
                profile.Email,
                profile.CountryCode,
                profile.PhoneNumber,
                profile.IsDeleted,
                profile.CreatedAtUtc,
                profile.UpdatedAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<UserProfileResponse>>(response);
    }
}
