using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Errors;
using SharedKernel.Application;
using SharedKernel.Application.Messaging;
using SharedKernel.Contracts;

namespace Identity.Application.Features.SocialLogin;

public sealed class SocialLoginCommandHandler : ICommandHandler<SocialLoginCommand, AuthTokenResponse>
{
    private readonly IEnumerable<ISocialAuthProviderPort> _socialProviders;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherPort _passwordHasher;
    private readonly IRbacAccessPort _rbacAccessPort;
    private readonly IUserProfilePort _userProfilePort;
    private readonly IJwtProviderPort _jwtProvider;

    public SocialLoginCommandHandler(
        IEnumerable<ISocialAuthProviderPort> socialProviders,
        IUserRepository userRepository,
        IPasswordHasherPort passwordHasher,
        IRbacAccessPort rbacAccessPort,
        IUserProfilePort userProfilePort,
        IJwtProviderPort jwtProvider)
    {
        _socialProviders = socialProviders;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _rbacAccessPort = rbacAccessPort;
        _userProfilePort = userProfilePort;
        _jwtProvider = jwtProvider;
    }

    public async Task<Result<AuthTokenResponse>> Handle(
        SocialLoginCommand request,
        CancellationToken cancellationToken)
    {
        var provider = _socialProviders.FirstOrDefault(p =>
            string.Equals(p.ProviderName, request.Provider.Trim(), StringComparison.OrdinalIgnoreCase));

        if (provider is null)
        {
            return Result.Failure<AuthTokenResponse>(
                IdentityErrors.UnsupportedSocialProvider(request.Provider));
        }

        var validationResult = await provider.ValidateAsync(request.Credential, cancellationToken);
        if (validationResult.IsFailure)
        {
            return Result.Failure<AuthTokenResponse>(validationResult.Error);
        }

        var extUser = validationResult.Value;
        if (string.IsNullOrWhiteSpace(extUser.Email) || !extUser.EmailVerified)
        {
            return Result.Failure<AuthTokenResponse>(
                IdentityErrors.SocialEmailRequired(provider.ProviderName));
        }

        string normalizedEmail = extUser.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByExternalLoginAsync(
            extUser.Provider,
            extUser.ProviderUserId,
            cancellationToken);

        if (user is null)
        {
            user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (user is not null)
            {
                // Pre-Hijacking Prevention:
                // If account existed with this email but was not verified, an unverified local
                // password might have been set by an attacker. Invalidate it and verify email.
                if (!user.IsEmailVerified)
                {
                    string randomHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N"));
                    user.UpdatePasswordHash(randomHash);
                    user.VerifyEmail();
                }

                bool loginExists = await _userRepository.ExternalLoginExistsAsync(
                    user.Id,
                    extUser.Provider,
                    cancellationToken);

                if (!loginExists)
                {
                    var externalLogin = ExternalLogin.Create(
                        user.Id,
                        extUser.Provider,
                        extUser.ProviderUserId,
                        extUser.Email);

                    await _userRepository.AddExternalLoginAsync(externalLogin, cancellationToken);
                }
            }
            else
            {
                string randomHash = _passwordHasher.HashPassword(Guid.NewGuid().ToString("N"));
                string firstName = !string.IsNullOrWhiteSpace(extUser.FirstName) ? extUser.FirstName.Trim() : "Social";
                string lastName = !string.IsNullOrWhiteSpace(extUser.LastName) ? extUser.LastName.Trim() : "User";

                user = User.Create(
                    normalizedEmail,
                    randomHash,
                    firstName,
                    lastName,
                    countryCode: null,
                    phoneNumber: null,
                    isEmailVerified: true);

                await _userRepository.AddUserAsync(user, cancellationToken);

                var externalLogin = ExternalLogin.Create(
                    user.Id,
                    extUser.Provider,
                    extUser.ProviderUserId,
                    extUser.Email);

                await _userRepository.AddExternalLoginAsync(externalLogin, cancellationToken);
                await _userRepository.SaveChangesAsync(cancellationToken);

                const string defaultRole = "StandardUser";
                await _rbacAccessPort.AssignDefaultRoleAsync(user.Id, defaultRole, cancellationToken);

                await _userProfilePort.CreateOrSyncProfileAsync(
                    user.Id,
                    user.Email,
                    $"{user.FirstName} {user.LastName}".Trim(),
                    null,
                    null,
                    cancellationToken);
            }
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
