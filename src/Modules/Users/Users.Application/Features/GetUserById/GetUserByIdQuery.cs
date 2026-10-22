using SharedKernel.Application.Messaging;
using Users.Application.DTOs;

namespace Users.Application.Features.GetUserById;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserProfileResponse>;
