using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using Users.Application.Abstractions.Ports;
using Users.Application.DTOs;
using Users.Domain.Errors;

namespace Users.Application.Features.GetUserByPhone;

public sealed class GetUserByPhoneQueryHandler : IQueryHandler<GetUserByPhoneQuery, UserProfileResponse>
{
    private readonly IUserProfileRepository _repository;

    public GetUserByPhoneQueryHandler(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserProfileResponse>> Handle(
        GetUserByPhoneQuery request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByPhoneAsync(request.CountryCode, request.PhoneNumber, cancellationToken);
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
