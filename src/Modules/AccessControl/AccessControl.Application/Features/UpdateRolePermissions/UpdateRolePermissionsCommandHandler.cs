using AccessControl.Application.Abstractions.Ports;
using AccessControl.Application.DTOs;
using AccessControl.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.UpdateRolePermissions;

public sealed class UpdateRolePermissionsCommandHandler
    : ICommandHandler<UpdateRolePermissionsCommand, UpdateRolePermissionsResponse>
{
    private static readonly string[] SuperAdminCorePermissions =
    {
        "auth:profile:read",
        "users:read",
        "users:manage",
        "roles:read",
        "roles:write",
        "roles:manage"
    };

    private readonly IRbacRepository _rbacRepository;

    public UpdateRolePermissionsCommandHandler(IRbacRepository rbacRepository)
    {
        _rbacRepository = rbacRepository;
    }

    public async Task<Result<UpdateRolePermissionsResponse>> Handle(
        UpdateRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _rbacRepository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<UpdateRolePermissionsResponse>(RbacErrors.RoleNotFound);
        }

        if (role.IsSystemRole && role.NormalizedName == "SUPERADMIN")
        {
            var missingCorePermissions = SuperAdminCorePermissions
                .Where(cp => !request.PermissionCodes.Contains(cp, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (missingCorePermissions.Count > 0)
            {
                return Result.Failure<UpdateRolePermissionsResponse>(RbacErrors.SystemRoleModificationForbidden);
            }
        }

        var distinctCodes = request.PermissionCodes.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var permissions = await _rbacRepository.GetPermissionsByCodesAsync(distinctCodes, cancellationToken);

        if (permissions.Count != distinctCodes.Count)
        {
            return Result.Failure<UpdateRolePermissionsResponse>(RbacErrors.InvalidPermissionCodes);
        }

        await _rbacRepository.UpdateRolePermissionsAsync(
            role.Id,
            permissions.Select(p => p.Id),
            cancellationToken);

        await _rbacRepository.SaveChangesAsync(cancellationToken);

        var updatedCodes = permissions.Select(p => p.Code).ToList();
        return Result.Success(new UpdateRolePermissionsResponse(role.Id, role.Name, updatedCodes));
    }
}
