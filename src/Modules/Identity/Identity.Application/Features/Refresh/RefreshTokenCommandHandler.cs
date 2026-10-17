using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace Identity.Application.Features.Refresh;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRbacAccessPort _rbacAccessPort;
    private readonly IJwtProviderPort _jwtProvider;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRbacAccessPort rbacAccessPort,
        IJwtProviderPort jwtProvider)
    {
        _userRepository = userRepository;
        _rbacAccessPort = rbacAccessPort;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || !existingToken.IsActive)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidRefreshToken);
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.UserNotFound);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.AccountNotActive);
        }

        var accessProfile = await _rbacAccessPort.GetUserAccessProfileAsync(user.Id, cancellationToken);

        var tokenPair = _jwtProvider.GenerateTokenPair(
            user,
            accessProfile.Roles,
            accessProfile.Permissions);

        existingToken.Revoke("Rotated", tokenPair.RefreshToken);

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.JwtId,
            tokenPair.RefreshTokenExpiresAtUtc);

        await _userRepository.AddRefreshTokenAsync(newRefreshToken, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var userContext = new UserContextResponse(
            user.Id,
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            accessProfile.Roles,
            accessProfile.Permissions);

        var response = new AuthTokenResponse(
            tokenPair.AccessToken,
            tokenPair.RefreshToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshTokenExpiresAtUtc,
            userContext);

        return Result.Success(response);
    }
}
