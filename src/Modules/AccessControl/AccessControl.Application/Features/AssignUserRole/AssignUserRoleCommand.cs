using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.AssignUserRole;

public sealed record AssignUserRoleCommand(
    Guid UserId,
    Guid RoleId) : ICommand<AssignUserRoleResponse>;
