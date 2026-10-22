using SharedKernel.Application.Messaging;
using Users.Application.DTOs;

namespace Users.Application.Features.CreateUser;

public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string? CountryCode,
    string? PhoneNumber) : ICommand<UserProfileResponse>;
