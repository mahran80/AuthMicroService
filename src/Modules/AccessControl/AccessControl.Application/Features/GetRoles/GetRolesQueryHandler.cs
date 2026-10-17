using AccessControl.Application.Abstractions.Ports;
using AccessControl.Application.DTOs;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.GetRoles;

public sealed class GetRolesQueryHandler : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleDto>>
{
    private readonly IRbacRepository _rbacRepository;

    public GetRolesQueryHandler(IRbacRepository rbacRepository)
    {
        _rbacRepository = rbacRepository;
    }

    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(
        GetRolesQuery request,
        CancellationToken cancellationToken)
    {
        var roles = await _rbacRepository.GetAllRolesAsync(cancellationToken);
        var resultList = new List<RoleDto>();

        foreach (var role in roles)
        {
            var permissions = await _rbacRepository.GetRolePermissionsAsync(role.Id, cancellationToken);
            var permissionCodes = permissions.Select(p => p.Code).ToList();

            resultList.Add(new RoleDto(
                role.Id,
                role.Name,
                role.IsSystemRole,
                permissionCodes));
        }

        return Result.Success<IReadOnlyList<RoleDto>>(resultList);
    }
}
