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

public sealed class TwitterAuthAdapter : ISocialAuthProviderPort
{
    private readonly HttpClient _httpClient;
    private readonly SocialAuthOptions _options;
    private readonly ILogger<TwitterAuthAdapter> _logger;

    public string ProviderName => "Twitter";

    public TwitterAuthAdapter(
        HttpClient httpClient,
        IOptions<SocialAuthOptions> options,
        ILogger<TwitterAuthAdapter> logger)
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

        string clientId = !string.IsNullOrWhiteSpace(_options.Twitter.ClientId)
            ? _options.Twitter.ClientId
            : _options.X.ClientId;

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogWarning("Twitter OAuth ClientId is not configured.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.twitter.com/2/users/me?user.fields=id,name,username");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", credential.Trim());
            request.Headers.UserAgent.ParseAdd("AuthMicroservice/1.0");

            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Twitter validation failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            var result = await response.Content.ReadFromJsonAsync<TwitterUserResponse>(cancellationToken: ct);
            var user = result?.Data;

            if (user is null || string.IsNullOrWhiteSpace(user.Id))
            {
                _logger.LogWarning("Twitter returned an invalid user profile.");
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            string email = !string.IsNullOrWhiteSpace(user.Email)
                ? user.Email
                : (!string.IsNullOrWhiteSpace(user.Username) ? $"{user.Username.ToLowerInvariant()}@twitter.com" : string.Empty);

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Twitter user {UserId} has no available email.", user.Id);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = user.Name ?? user.Username ?? "TwitterUser";
            string lastName = string.Empty;

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                var parts = user.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : user.Name;
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
            _logger.LogError(ex, "Unexpected error validating Twitter credential.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }

    private sealed class TwitterUserResponse
    {
        [JsonPropertyName("data")]
        public TwitterUser? Data { get; set; }
    }

    private sealed class TwitterUser
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
