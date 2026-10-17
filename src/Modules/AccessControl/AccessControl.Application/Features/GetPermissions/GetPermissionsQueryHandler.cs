using AccessControl.Application.Abstractions.Ports;
using AccessControl.Application.DTOs;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.GetPermissions;

public sealed class GetPermissionsQueryHandler : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    private readonly IRbacRepository _rbacRepository;

    public GetPermissionsQueryHandler(IRbacRepository rbacRepository)
    {
        _rbacRepository = rbacRepository;
    }

    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _rbacRepository.GetAllPermissionsAsync(cancellationToken);

        var dtoList = permissions
            .Select(p => new PermissionDto(p.Id, p.Code, p.DisplayName, p.ModuleGroup, p.Description))
            .ToList();

        return Result.Success<IReadOnlyList<PermissionDto>>(dtoList);
    }
}
