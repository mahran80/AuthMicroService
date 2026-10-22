using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;
using Users.Application.Abstractions.Ports;
using Users.Domain.Errors;

namespace Users.Application.Features.DeleteUser;

public sealed class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IUserProfileRepository _repository;
    private readonly IUserIdentityPort _userIdentityPort;

    public DeleteUserCommandHandler(
        IUserProfileRepository repository,
        IUserIdentityPort userIdentityPort)
    {
        _repository = repository;
        _userIdentityPort = userIdentityPort;
    }

    public async Task<Result> Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _repository.GetByIdAsync(request.UserId, cancellationToken);
        if (profile is null)
        {
            return Result.Failure(UsersErrors.ProfileNotFound);
        }

        if (profile.IsDeleted)
        {
            return Result.Failure(UsersErrors.ProfileAlreadyDeleted);
        }

        profile.SoftDelete();
        await _repository.SaveChangesAsync(cancellationToken);

        await _userIdentityPort.UpdateUserStatusAndRevokeTokensAsync(
            request.UserId,
            "Deactivated",
            cancellationToken);

        return Result.Success();
    }
}
