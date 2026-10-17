using AccessControl.Application.DTOs;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace AccessControl.Application.Features.UpdateUserStatus;

public sealed class UpdateUserStatusCommandHandler
    : ICommandHandler<UpdateUserStatusCommand, UpdateUserStatusResponse>
{
    private readonly IUserIdentityPort _userIdentityPort;

    public UpdateUserStatusCommandHandler(IUserIdentityPort userIdentityPort)
    {
        _userIdentityPort = userIdentityPort;
    }

    public async Task<Result<UpdateUserStatusResponse>> Handle(
        UpdateUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        var result = await _userIdentityPort.UpdateUserStatusAndRevokeTokensAsync(
            request.UserId,
            request.Status,
            cancellationToken);

        if (result.IsFailure)
        {
            return Result.Failure<UpdateUserStatusResponse>(result.Error);
        }

        return Result.Success(new UpdateUserStatusResponse(
            request.UserId,
            request.Status,
            result.Value));
    }
}
