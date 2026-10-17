using AccessControl.Application.Abstractions.Ports;
using AccessControl.Application.DTOs;
using AccessControl.Domain.Entities;
using AccessControl.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace AccessControl.Application.Features.AssignUserRole;

public sealed class AssignUserRoleCommandHandler : ICommandHandler<AssignUserRoleCommand, AssignUserRoleResponse>
{
    private readonly IRbacRepository _rbacRepository;
    private readonly IUserIdentityPort _userIdentityPort;

    public AssignUserRoleCommandHandler(
        IRbacRepository rbacRepository,
        IUserIdentityPort userIdentityPort)
    {
        _rbacRepository = rbacRepository;
        _userIdentityPort = userIdentityPort;
    }

    public async Task<Result<AssignUserRoleResponse>> Handle(
        AssignUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _rbacRepository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AssignUserRoleResponse>(RbacErrors.RoleNotFound);
        }

        bool userExists = await _userIdentityPort.CheckUserExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            return Result.Failure<AssignUserRoleResponse>(RbacErrors.UserNotFound);
        }

        bool alreadyAssigned = await _rbacRepository.UserRoleExistsAsync(
            request.UserId,
            request.RoleId,
            cancellationToken);

        if (!alreadyAssigned)
        {
            var userRole = UserRole.Create(request.UserId, request.RoleId);
            await _rbacRepository.AssignUserRoleAsync(userRole, cancellationToken);
            await _rbacRepository.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new AssignUserRoleResponse(request.UserId, role.Id, role.Name));
    }
}
