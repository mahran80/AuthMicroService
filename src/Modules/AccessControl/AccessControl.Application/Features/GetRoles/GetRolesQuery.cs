using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.GetRoles;

public sealed record GetRolesQuery : IQuery<IReadOnlyList<RoleDto>>;
