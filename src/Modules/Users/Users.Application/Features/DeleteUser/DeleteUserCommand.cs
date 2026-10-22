using SharedKernel.Application.Messaging;

namespace Users.Application.Features.DeleteUser;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;
