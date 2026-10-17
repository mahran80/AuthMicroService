using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.UpdateRolePermissions;

public sealed record UpdateRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyList<string> PermissionCodes) : ICommand<UpdateRolePermissionsResponse>;
