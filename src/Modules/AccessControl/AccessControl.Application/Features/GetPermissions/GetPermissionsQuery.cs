using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.GetPermissions;

public sealed record GetPermissionsQuery : IQuery<IReadOnlyList<PermissionDto>>;
