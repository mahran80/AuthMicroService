using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;

namespace Identity.Application.Features.Revoke;

public sealed class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand, RevokeTokenResponse>
{
    private readonly IUserRepository _userRepository;

    public RevokeTokenCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<RevokeTokenResponse>> Handle(
        RevokeTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<RevokeTokenResponse>(IdentityErrors.InvalidRefreshToken);
        }

        existingToken.Revoke("UserLogout");
        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result.Success(new RevokeTokenResponse("Refresh token has been successfully revoked."));
    }
}
