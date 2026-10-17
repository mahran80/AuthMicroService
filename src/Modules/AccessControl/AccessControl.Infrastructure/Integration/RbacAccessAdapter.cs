using AccessControl.Domain.Entities;
using AccessControl.Domain.Errors;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Application;
using SharedKernel.Contracts;

namespace AccessControl.Infrastructure.Integration;

public sealed class RbacAccessAdapter : IRbacAccessPort
{
    private readonly AccessControlDbContext _context;

    public RbacAccessAdapter(AccessControlDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccessProfileDto> GetUserAccessProfileAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(cancellationToken);

        if (roleIds.Count == 0)
        {
            return new UserAccessProfileDto(Array.Empty<string>(), Array.Empty<string>());
        }

        var roleNames = await _context.Roles
            .Where(r => roleIds.Contains(r.Id))
            .Select(r => r.Name)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissionIds = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.PermissionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var permissionCodes = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id))
            .Select(p => p.Code)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new UserAccessProfileDto(roleNames, permissionCodes);
    }

    public async Task<Result> AssignDefaultRoleAsync(
        Guid userId,
        string defaultRoleName,
        CancellationToken cancellationToken = default)
    {
        string normalized = defaultRoleName.Trim().ToUpperInvariant();

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == normalized, cancellationToken);

        if (role is null)
        {
            return Result.Failure(RbacErrors.RoleNotFound);
        }

        bool alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id, cancellationToken);

        if (!alreadyAssigned)
        {
            var userRole = UserRole.Create(userId, role.Id);
            await _context.UserRoles.AddAsync(userRole, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
