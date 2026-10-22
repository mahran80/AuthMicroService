using System.Text.Json;
using Identity.Application.Abstractions.Ports;
using Identity.Domain.Errors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel.Application;

namespace Identity.Infrastructure.Notifications;

public sealed class UltraMsgWhatsAppAdapter : IWhatsAppNotificationPort
{
    private readonly HttpClient _httpClient;
    private readonly UltraMsgOptions _options;
    private readonly ILogger<UltraMsgWhatsAppAdapter> _logger;

    public UltraMsgWhatsAppAdapter(
        HttpClient httpClient,
        IOptions<UltraMsgOptions> options,
        ILogger<UltraMsgWhatsAppAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendOtpMessageAsync(
        string countryCode,
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.InstanceId) || string.IsNullOrWhiteSpace(_options.Token))
        {
            _logger.LogWarning(
                "UltraMsg InstanceId/Token are not configured. Skipping WhatsApp message to {CountryCode}{PhoneNumber} for local development.",
                countryCode,
                phoneNumber);

            return Result.Success();
        }

        string recipient = $"{countryCode.Trim()}{phoneNumber.Trim()}";
        string baseUrl = string.IsNullOrWhiteSpace(_options.BaseUrl)
            ? "https://api.ultramsg.com"
            : _options.BaseUrl.TrimEnd('/');

        string endpoint = $"{baseUrl}/{_options.InstanceId}/messages/chat";

        var formValues = new Dictionary<string, string>
        {
            ["token"] = _options.Token,
            ["to"] = recipient,
            ["body"] = $"Your verification code is: {otpCode}"
        };

        using var content = new FormUrlEncodedContent(formValues);

        try
        {
            using var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "UltraMsg HTTP delivery failed with status {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    responseBody);

                return Result.Failure(IdentityErrors.NotificationDeliveryFailed);
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(responseBody);
                if (jsonDoc.RootElement.TryGetProperty("sent", out var sentElement) &&
                    sentElement.ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("UltraMsg reported message not sent: {Response}", responseBody);
                    return Result.Failure(IdentityErrors.NotificationDeliveryFailed);
                }
            }
            catch (JsonException)
            {
                // Non-JSON response, treated as successful HTTP status
            }

            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error communicating with UltraMsg for recipient {Recipient}", recipient);
            return Result.Failure(IdentityErrors.NotificationDeliveryFailed);
        }
    }
}
