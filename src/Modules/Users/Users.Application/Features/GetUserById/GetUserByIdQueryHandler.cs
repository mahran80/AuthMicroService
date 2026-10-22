using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using Users.Application.Abstractions.Ports;
using Users.Application.DTOs;
using Users.Domain.Errors;

namespace Users.Application.Features.GetUserById;

public sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserProfileResponse>
{
    private readonly IUserProfileRepository _repository;

    public GetUserByIdQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        GetUserByIdQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (profile is null || profile.IsDeleted)
        {
            return Result.Failure<UserProfileResponse>(UsersErrors.ProfileNotFound);
        }

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
