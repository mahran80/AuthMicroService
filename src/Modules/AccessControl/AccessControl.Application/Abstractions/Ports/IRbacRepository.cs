using AccessControl.Domain.Entities;

namespace AccessControl.Application.Abstractions.Ports;

public interface IRbacRepository
{
    Task<bool> RoleExistsByNameAsync(string name, CancellationToken ct = default);
    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default);
    Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetPermissionsByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default);
    Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken ct = default);
    Task AddRoleAsync(Role role, CancellationToken ct = default);
    Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default);
    Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task AssignUserRoleAsync(UserRole userRole, CancellationToken ct = default);
    Task<(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions)> GetUserAccessProfileAsync(Guid userId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
