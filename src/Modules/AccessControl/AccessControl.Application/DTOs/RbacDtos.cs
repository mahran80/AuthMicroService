namespace AccessControl.Application.DTOs;

public sealed record CreateRoleResponse(
    Guid RoleId,
    string Name,
    string NormalizedName,
    string? Description,
    bool IsSystemRole);

public sealed record RoleDto(
    Guid RoleId,
    string Name,
    bool IsSystemRole,
    IReadOnlyList<string> Permissions);

public sealed record PermissionDto(
    Guid Id,
    string Code,
    string DisplayName,
    string ModuleGroup,
    string? Description);

public sealed record UpdateRolePermissionsResponse(
    Guid RoleId,
    string RoleName,
    IReadOnlyList<string> UpdatedPermissions);

public sealed record AssignUserRoleResponse(
    Guid UserId,
    Guid AssignedRoleId,
    string AssignedRoleName);

public sealed record UpdateUserStatusResponse(
    Guid UserId,
    string NewStatus,
    int RevokedActiveTokensCount);
