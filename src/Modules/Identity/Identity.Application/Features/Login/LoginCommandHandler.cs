using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace Identity.Application.Features.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, AuthTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherPort _passwordHasher;
    private readonly IRbacAccessPort _rbacAccessPort;
    private readonly IJwtProviderPort _jwtProvider;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasherPort passwordHasher,
        IRbacAccessPort rbacAccessPort,
        IJwtProviderPort jwtProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _rbacAccessPort = rbacAccessPort;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthTokenResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        string normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidCredentials);
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.AccountNotActive);
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthTokenResponse>(IdentityErrors.InvalidCredentials);
        }

        var accessProfile = await _rbacAccessPort.GetUserAccessProfileAsync(user.Id, cancellationToken);

        var tokenPair = _jwtProvider.GenerateTokenPair(
            user,
            accessProfile.Roles,
            accessProfile.Permissions);

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshToken,
            tokenPair.JwtId,
            tokenPair.RefreshTokenExpiresAtUtc);

        await _userRepository.AddRefreshTokenAsync(refreshToken, cancellationToken);
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
