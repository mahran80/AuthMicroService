using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Identity.Application.Abstractions.Ports;
using Identity.Application.DTOs;
using Identity.Domain.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Application;

namespace Identity.Infrastructure.Social;

public sealed class LinkedInAuthAdapter : ISocialAuthProviderPort
{
    private readonly HttpClient _httpClient;
    private readonly SocialAuthOptions _options;
    private readonly ILogger<LinkedInAuthAdapter> _logger;

    public string ProviderName => "LinkedIn";

    public LinkedInAuthAdapter(
        HttpClient httpClient,
        IOptions<SocialAuthOptions> options,
        ILogger<LinkedInAuthAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<ExternalUserInfo>> ValidateAsync(string credential, CancellationToken ct = default)
    {
        // Fix Marwan's inverted logic bug
        if (string.IsNullOrWhiteSpace(credential))
        {
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }

        var linkedInConfig = _options.LinkedIn;
        if (string.IsNullOrWhiteSpace(linkedInConfig.ClientId) ||
            string.IsNullOrWhiteSpace(linkedInConfig.ClientSecret) ||
            string.IsNullOrWhiteSpace(linkedInConfig.RedirectUri))
        {
            _logger.LogWarning("LinkedIn OAuth configuration is incomplete.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        try
        {
            string? accessToken = await ExchangeCodeForTokenAsync(credential, linkedInConfig, ct);
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            var user = await GetUserAsync(accessToken, ct);
            if (user is null || string.IsNullOrWhiteSpace(user.Sub))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                _logger.LogWarning("LinkedIn user {Sub} has no available email address.", user.Sub);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = user.GivenName ?? string.Empty;
            string lastName = user.FamilyName ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(user.Name))
            {
                var parts = user.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : user.Name;
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }

            return Result.Success(new ExternalUserInfo(
                Provider: ProviderName,
                ProviderUserId: user.Sub,
                Email: user.Email,
                FirstName: firstName,
                LastName: lastName,
                EmailVerified: user.EmailVerified ?? true));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with LinkedIn OAuth.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(
        string code,
        LinkedInOptions config,
        CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = config.RedirectUri,
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.linkedin.com/oauth/v2/accessToken")
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("LinkedIn token endpoint returned {StatusCode}.", response.StatusCode);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<LinkedInTokenResponse>(cancellationToken: ct);
        return result?.AccessToken;
    }

    private async Task<LinkedInUser?> GetUserAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("LinkedIn /v2/userinfo call returned {StatusCode}.", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LinkedInUser>(cancellationToken: ct);
    }

    private sealed class LinkedInTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class LinkedInUser
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("email_verified")]
        public bool? EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }
    }
}
