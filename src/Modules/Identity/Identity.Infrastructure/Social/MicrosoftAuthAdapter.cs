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

public sealed class MicrosoftAuthAdapter : ISocialAuthProviderPort
{
    private readonly HttpClient _httpClient;
    private readonly SocialAuthOptions _options;
    private readonly ILogger<MicrosoftAuthAdapter> _logger;

    public string ProviderName => "Microsoft";

    public MicrosoftAuthAdapter(
        HttpClient httpClient,
        IOptions<SocialAuthOptions> options,
        ILogger<MicrosoftAuthAdapter> logger)
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

        var msConfig = _options.Microsoft;
        string trimmedCredential = credential.Trim();
        bool isDirectAccessToken = trimmedCredential.StartsWith("ey", StringComparison.Ordinal) ||
                                   trimmedCredential.StartsWith("Ew", StringComparison.Ordinal);

        if (string.IsNullOrWhiteSpace(msConfig.ClientId))
        {
            _logger.LogWarning("Microsoft OAuth configuration is incomplete: ClientId is missing.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        string tenant = !string.IsNullOrWhiteSpace(msConfig.TenantId) ? msConfig.TenantId : "common";

        try
        {
            string? accessToken = null;

            if (!string.IsNullOrWhiteSpace(msConfig.ClientSecret) &&
                !string.IsNullOrWhiteSpace(msConfig.RedirectUri) &&
                !isDirectAccessToken)
            {
                accessToken = await ExchangeCodeForTokenAsync(trimmedCredential, msConfig, tenant, ct);
            }
            else
            {
                accessToken = trimmedCredential;
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            var user = await GetUserAsync(accessToken, ct);
            if (user is null || string.IsNullOrWhiteSpace(user.Id))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            string? email = !string.IsNullOrWhiteSpace(user.Mail)
                ? user.Mail
                : user.UserPrincipalName;

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Microsoft user {Id} has no usable email address.", user.Id);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = user.GivenName ?? string.Empty;
            string lastName = user.Surname ?? string.Empty;

            if (string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(user.DisplayName))
            {
                var parts = user.DisplayName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : user.DisplayName;
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }

            return Result.Success(new ExternalUserInfo(
                Provider: ProviderName,
                ProviderUserId: user.Id,
                Email: email,
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
            _logger.LogError(ex, "Unexpected error communicating with Microsoft Graph API.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(
        string code,
        MicrosoftOptions config,
        string tenant,
        CancellationToken ct)
    {
        var tokenUrl = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";

        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = config.RedirectUri,
            ["grant_type"] = "authorization_code",
            ["scope"] = "openid profile email User.Read"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(parameters)
        };

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Microsoft token exchange failed with status {StatusCode}.", response.StatusCode);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<MicrosoftTokenResponse>(cancellationToken: ct);
        return result?.AccessToken;
    }

    private async Task<MicrosoftUser?> GetUserAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://graph.microsoft.com/v1.0/me?$select=id,displayName,givenName,surname,mail,userPrincipalName");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Microsoft Graph /me call failed with status {StatusCode}.", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<MicrosoftUser>(cancellationToken: ct);
    }

    private sealed class MicrosoftTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    private sealed class MicrosoftUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("givenName")]
        public string? GivenName { get; set; }

        [JsonPropertyName("surname")]
        public string? Surname { get; set; }

        [JsonPropertyName("mail")]
        public string? Mail { get; set; }

        [JsonPropertyName("userPrincipalName")]
        public string? UserPrincipalName { get; set; }
    }
}
