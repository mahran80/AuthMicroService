namespace AccessControl.Domain.Entities;

public sealed class RolePermission
{
    public Guid RoleId { get; private init; }
    public Guid PermissionId { get; private init; }
    public DateTime GrantedAtUtc { get; private init; }

    private RolePermission()
    {
    }

    private RolePermission(Guid roleId, Guid permissionId, DateTime grantedAtUtc)
    {
        RoleId = roleId;
        PermissionId = permissionId;
        GrantedAtUtc = grantedAtUtc;
    }

    public static RolePermission Create(Guid roleId, Guid permissionId, DateTime? grantedAtUtc = null)
    {
        return new RolePermission(roleId, permissionId, grantedAtUtc ?? DateTime.UtcNow);
    }
}
