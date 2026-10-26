using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Application;

namespace Identity.Infrastructure.Social;

public sealed class FacebookAuthAdapter : ISocialAuthProviderPort
{
    private readonly HttpClient _httpClient;
    private readonly SocialAuthOptions _options;
    private readonly ILogger<FacebookAuthAdapter> _logger;

    public string ProviderName => "Facebook";

    public FacebookAuthAdapter(
        HttpClient httpClient,
        IOptions<SocialAuthOptions> options,
        ILogger<FacebookAuthAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ExternalUserInfo>> ValidateAsync(string credential, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }

        if (string.IsNullOrWhiteSpace(_options.Facebook.AppId))
        {
            _logger.LogWarning("Facebook AppId is not configured.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        try
        {
            // If AppId and AppSecret are configured, optionally verify debug_token
            if (!string.IsNullOrWhiteSpace(_options.Facebook.AppId) && !string.IsNullOrWhiteSpace(_options.Facebook.AppSecret))
            {
                var debugUrl = $"https://graph.facebook.com/debug_token?input_token={Uri.EscapeDataString(credential)}&access_token={_options.Facebook.AppId}|{_options.Facebook.AppSecret}";
                using var debugResponse = await _httpClient.GetAsync(debugUrl, ct);
                if (debugResponse.IsSuccessStatusCode)
                {
                    var debugResult = await debugResponse.Content.ReadFromJsonAsync<FacebookDebugTokenResponse>(cancellationToken: ct);
                    if (debugResult?.Data is not null && !debugResult.Data.IsValid)
                    {
                        _logger.LogWarning("Facebook token debug validation marked token as invalid.");
                        return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
                    }
                }
            }

            var url = $"https://graph.facebook.com/me?fields=id,name,email,first_name,last_name&access_token={Uri.EscapeDataString(credential)}";
            using var response = await _httpClient.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Facebook validation failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            var facebookUser = await response.Content.ReadFromJsonAsync<FacebookUserResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                ct);

            if (facebookUser is null || string.IsNullOrWhiteSpace(facebookUser.Id))
            {
                _logger.LogWarning("Facebook returned an invalid or empty user profile.");
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            // Fix Critical Flaw: Disallow empty/missing emails
            if (string.IsNullOrWhiteSpace(facebookUser.Email))
            {
                _logger.LogWarning("Facebook user {Id} did not provide an email address.", facebookUser.Id);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = facebookUser.FirstName ?? string.Empty;
            string lastName = facebookUser.LastName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(facebookUser.Name))
            {
                var parts = facebookUser.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : facebookUser.Name;
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }

            return Result.Success(new ExternalUserInfo(
                Provider: ProviderName,
                ProviderUserId: facebookUser.Id,
                Email: facebookUser.Email,
                FirstName: firstName,
                LastName: lastName,
                EmailVerified: true));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error validating Facebook credential.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }

    private sealed class FacebookDebugTokenResponse
    {
        [JsonPropertyName("data")]
        public FacebookDebugTokenData? Data { get; set; }
    }

    private sealed class FacebookDebugTokenData
    {
        [JsonPropertyName("is_valid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }

        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }

    private sealed class FacebookUserResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }
    }
}
