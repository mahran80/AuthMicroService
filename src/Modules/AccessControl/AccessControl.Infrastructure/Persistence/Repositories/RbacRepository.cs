using AccessControl.Application.Abstractions.Ports;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Persistence.Repositories;

public sealed class RbacRepository : IRbacRepository
{
    private readonly AccessControlDbContext _context;

    public RbacRepository(AccessControlDbContext context)
    {
        _context = context;
    }

    public async Task<bool> RoleExistsByNameAsync(string name, CancellationToken ct = default)
    {
        string normalized = name.Trim().ToUpperInvariant();
        return await _context.Roles.AnyAsync(r => r.NormalizedName == normalized, ct);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);
    }

    public async Task<Role?> GetRoleByNameAsync(string name, CancellationToken ct = default)
    {
        string normalized = name.Trim().ToUpperInvariant();
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.NormalizedName == normalized, ct);
    }

    public async Task<IReadOnlyList<Role>> GetAllRolesAsync(CancellationToken ct = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        return await _context.Permissions
            .OrderBy(p => p.ModuleGroup)
            .ThenBy(p => p.Code)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetPermissionsByCodesAsync(IEnumerable<string> codes, CancellationToken ct = default)
    {
        var codeSet = codes.Select(c => c.Trim().ToLowerInvariant()).ToHashSet();
        return await _context.Permissions
            .Where(p => codeSet.Contains(p.Code.ToLower()))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken ct = default)
    {
        var permissionIds = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync(ct);

        return await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .OrderBy(p => p.Code)
            .ToListAsync(ct);
    }

    public async Task AddRoleAsync(Role role, CancellationToken ct = default)
    {
        await _context.Roles.AddAsync(role, ct);
    }

    public async Task UpdateRolePermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken ct = default)
    {
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync(ct);

        _context.RolePermissions.RemoveRange(existing);

        var newRolePermissions = permissionIds
            .Distinct()
            .Select(permId => RolePermission.Create(roleId, permId));

        await _context.RolePermissions.AddRangeAsync(newRolePermissions, ct);
    }

    public async Task<bool> UserRoleExistsAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        return await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
    }

    public async Task AssignUserRoleAsync(UserRole userRole, CancellationToken ct = default)
    {
        await _context.UserRoles.AddAsync(userRole, ct);
    }

    public async Task<(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions)> GetUserAccessProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        if (roleIds.Count == 0)
        {
            return (Array.Empty<string>(), Array.Empty<string>());
        }

        var roles = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .Distinct()
            .ToListAsync(ct);

        var permissionIds = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(ct);

        var permissions = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .Distinct()
            .ToListAsync(ct);

        return (roles, permissions);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
