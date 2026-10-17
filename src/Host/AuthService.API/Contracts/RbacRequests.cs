namespace AuthService.API.Contracts;

public sealed record CreateRoleRequest(
    string Name,
    string? Description);

public sealed record UpdateRolePermissionsRequest(
    List<string> PermissionCodes);

public sealed record AssignUserRoleRequest(
    Guid RoleId);

public sealed record UpdateUserStatusRequest(
    string Status);