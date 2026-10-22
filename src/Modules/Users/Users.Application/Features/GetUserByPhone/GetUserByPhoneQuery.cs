using SharedKernel.Application.Messaging;
using Users.Application.DTOs;

namespace Users.Application.Features.GetUserByPhone;

public sealed record GetUserByPhoneQuery(
    string? CountryCode,
    string PhoneNumber) : IQuery<UserProfileResponse>;
