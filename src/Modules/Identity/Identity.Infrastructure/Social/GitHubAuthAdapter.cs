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

public sealed class GitHubAuthAdapter : ISocialAuthProviderPort
{
    private readonly HttpClient _httpClient;
    private readonly SocialAuthOptions _options;
    private readonly ILogger<GitHubAuthAdapter> _logger;

    public string ProviderName => "GitHub";

    public GitHubAuthAdapter(
        HttpClient httpClient,
        IOptions<SocialAuthOptions> options,
        ILogger<GitHubAuthAdapter> logger)
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

        var githubOptions = _options.GitHub;
        string trimmedCredential = credential.Trim();
        bool isDirectAccessToken = trimmedCredential.StartsWith("gho_", StringComparison.OrdinalIgnoreCase) ||
                                   trimmedCredential.StartsWith("ghp_", StringComparison.OrdinalIgnoreCase) ||
                                   trimmedCredential.StartsWith("github_pat_", StringComparison.OrdinalIgnoreCase);

        if (!isDirectAccessToken && (string.IsNullOrWhiteSpace(githubOptions.ClientId) ||
            string.IsNullOrWhiteSpace(githubOptions.ClientSecret)))
        {
            _logger.LogWarning("GitHub OAuth ClientId or ClientSecret is not configured.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName));
        }

        try
        {
            string? accessToken = isDirectAccessToken
                ? trimmedCredential
                : await ExchangeCodeForTokenAsync(trimmedCredential, githubOptions, ct);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            var user = await GetUserAsync(accessToken, ct);
            if (user is null || user.Id == 0)
            {
                return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
            }

            string? email = user.Email;
            bool isVerified = true;

            // If public email is not set, fetch verified emails from the GitHub user emails endpoint
            if (string.IsNullOrWhiteSpace(email))
            {
                email = await GetPrimaryVerifiedEmailAsync(accessToken, ct);
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GitHub user {Login} has no available verified email address.", user.Login);
                return Result.Failure<ExternalUserInfo>(IdentityErrors.SocialEmailRequired(ProviderName));
            }

            string firstName = string.Empty;
            string lastName = string.Empty;

            if (!string.IsNullOrWhiteSpace(user.Name))
            {
                var parts = user.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = parts.Length > 0 ? parts[0] : user.Name;
                lastName = parts.Length > 1 ? parts[1] : string.Empty;
            }
            else
            {
                firstName = user.Login ?? "GitHubUser";
            }

            return Result.Success(new ExternalUserInfo(
                Provider: ProviderName,
                ProviderUserId: user.Id.ToString(),
                Email: email,
                FirstName: firstName,
                LastName: lastName,
                EmailVerified: isVerified));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with GitHub API.");
            return Result.Failure<ExternalUserInfo>(IdentityErrors.InvalidSocialCredential(ProviderName));
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(
        string code,
        GitHubOptions config,
        CancellationToken ct)
    {
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["code"] = code
        };

        if (!string.IsNullOrWhiteSpace(config.RedirectUri))
        {
            parameters["redirect_uri"] = config.RedirectUri;
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(parameters)
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub token exchange returned status {StatusCode}.", response.StatusCode);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<GitHubTokenResponse>(cancellationToken: ct);
        if (!string.IsNullOrWhiteSpace(result?.Error))
        {
            _logger.LogWarning("GitHub token exchange returned error: {Error}", result.Error);
            return null;
        }

        return result?.AccessToken;
    }

    private async Task<GitHubUser?> GetUserAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("AuthMicroservice/1.0");

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHub /user call returned status {StatusCode}.", response.StatusCode);
            return null;
        }

        return await response.Content.ReadFromJsonAsync<GitHubUser>(cancellationToken: ct);
    }

    private async Task<string?> GetPrimaryVerifiedEmailAsync(string accessToken, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.ParseAdd("AuthMicroservice/1.0");

        using var response = await _httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var emails = await response.Content.ReadFromJsonAsync<List<GitHubEmailItem>>(cancellationToken: ct);
        if (emails is null || emails.Count == 0)
        {
            return null;
        }

        var primary = emails.FirstOrDefault(e => e.Primary && e.Verified);
        if (primary is not null && !string.IsNullOrWhiteSpace(primary.Email))
        {
            return primary.Email;
        }

        var verified = emails.FirstOrDefault(e => e.Verified);
        return verified?.Email;
    }

    private sealed class GitHubTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    private sealed class GitHubUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("login")]
        public string? Login { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    private sealed class GitHubEmailItem
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("primary")]
        public bool Primary { get; set; }

        [JsonPropertyName("verified")]
        public bool Verified { get; set; }
    }
}
