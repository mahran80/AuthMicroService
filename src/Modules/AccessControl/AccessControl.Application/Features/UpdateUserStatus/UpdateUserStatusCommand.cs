using AccessControl.Application.DTOs;
using SharedKernel.Application.Messaging;

namespace AccessControl.Application.Features.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(
    Guid UserId,
    string Status) : ICommand<UpdateUserStatusResponse>;
