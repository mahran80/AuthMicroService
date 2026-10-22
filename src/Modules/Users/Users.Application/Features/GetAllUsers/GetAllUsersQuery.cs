using SharedKernel.Application.Messaging;
using Users.Application.DTOs;

namespace Users.Application.Features.GetAllUsers;

public sealed record GetAllUsersQuery() : IQuery<IReadOnlyList<UserProfileResponse>>;
