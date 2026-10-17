using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.CreateRole;

public sealed record CreateRoleCommand(
    string Name,
    string? Description) : ICommand<CreateRoleResponse>;
