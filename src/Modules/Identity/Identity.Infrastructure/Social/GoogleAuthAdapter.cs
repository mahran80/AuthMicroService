using Google.Apis.Auth;
using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Application;

namespace Identity.Infrastructure.Social;

public sealed class GoogleAuthAdapter : ISocialAuthProviderPort
{
    private readonly SocialAuthOptions _options;
    private readonly ILogger<GoogleAuthAdapter> _logger;

    public string ProviderName => "Google";

    public GoogleAuthAdapter(
        IOptions<SocialAuthOptions> options,
        ILogger<GoogleAuthAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ExternalUserInfo>> ValidateAsync(string credential, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }

        string clientId = _options.Google.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Google ClientId is not configured.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });

            if (payload is null || string.IsNullOrWhiteSpace(payload.Subject))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            if (!payload.EmailVerified)
            {
                _logger.LogWarning("Google account email is not verified: {Email}", payload.Email);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = payload.GivenName ?? string.Empty;
            string lastName = payload.FamilyName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(payload.Name))
            {
                var parts = payload.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : payload.Name;
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }

            return Result.Success(new ExternalUserInfo(
                Provider: ProviderName,
                ProviderUserId: payload.Subject,
                Email: payload.Email,
                FirstName: firstName,
                LastName: lastName,
                EmailVerified: payload.EmailVerified));
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Google token validation failed: {Message}", ex.Message);
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating Google credential.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }
}
