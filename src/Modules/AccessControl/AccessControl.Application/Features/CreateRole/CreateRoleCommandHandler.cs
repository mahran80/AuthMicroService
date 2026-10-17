using AccessControl.Application.Abstractions.Ports;
using AccessControl.Application.DTOs;
using AccessControl.Domain.Entities;
using AccessControl.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.CreateRole;

public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, CreateRoleResponse>
{
    private readonly IRbacRepository _rbacRepository;

    public CreateRoleCommandHandler(IRbacRepository rbacRepository)
    {
        _rbacRepository = rbacRepository;
    }

    public async Task<Result<CreateRoleResponse>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (await _rbacRepository.RoleExistsByNameAsync(request.Name, cancellationToken))
        {
            return Result.Failure<CreateRoleResponse>(RbacErrors.RoleAlreadyExists);
        }

        var role = Role.Create(request.Name, request.Description, isSystemRole: false);

        await _rbacRepository.AddRoleAsync(role, cancellationToken);
        await _rbacRepository.SaveChangesAsync(cancellationToken);

        var response = new CreateRoleResponse(
            role.Id,
            role.Name,
            role.NormalizedName,
            role.Description,
            role.IsSystemRole);

        return Result.Success(response);
    }
}
