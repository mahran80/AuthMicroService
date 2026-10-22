using SharedKernel.Application.Messaging;
using Users.Application.DTOs;

namespace Users.Application.Features.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string FullName,
    string Email,
    string? CountryCode,
    string? PhoneNumber) : ICommand<UserProfileResponse>;
