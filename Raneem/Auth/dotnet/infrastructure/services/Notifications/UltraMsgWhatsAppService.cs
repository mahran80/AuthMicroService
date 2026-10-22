using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Notifications.WhatsApp.Application.Dtos;
using Auth.Application.Ports.Outbound;
using Notifications.WhatsApp.Infrastructure.Configuration;

namespace Notifications.WhatsApp.Infrastructure.Services
{
    /// <summary>
    /// Sends WhatsApp messages through the UltraMsg REST API
    /// (https://api.ultramsg.com/{instance_id}/messages/chat).
    /// Register with a typed HttpClient so BaseAddress/timeouts are configured once in DI.
    /// </summary>
    public class UltraMsgWhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly UltraMsgOptions _options;
        private readonly ILogger<UltraMsgWhatsAppService> _logger;

        public UltraMsgWhatsAppService(
            HttpClient httpClient,
            IOptions<UltraMsgOptions> options,
            ILogger<UltraMsgWhatsAppService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<WhatsAppMessageResult> SendTextMessageAsync(
            WhatsAppMessageRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.InstanceId) || string.IsNullOrWhiteSpace(_options.Token))
            {
                _logger.LogError("UltraMsg InstanceId/Token are not configured");
                return WhatsAppMessageResult.Failure("WhatsApp provider is not configured");
            }

            // UltraMsg expects application/x-www-form-urlencoded, not JSON.
            var formValues = new Dictionary<string, string>
            {
                ["token"] = _options.Token,
                ["to"] = request.To,
                ["body"] = request.Body
            };

            if (request.Priority.HasValue)
            {
                formValues["priority"] = request.Priority.Value.ToString();
            }

            using var content = new FormUrlEncodedContent(formValues);

            // BaseAddress is set to "{BaseUrl}/{instance_id}/" in DI registration below,
            // so the relative path here is just the endpoint.
            var relativeUrl = "messages/chat";

            try
            {
                using var response = await _httpClient.PostAsync(relativeUrl, content, cancellationToken);
                var raw = await response.Content.ReadAsStringAsync(cancellationToken);
_logger.LogInformation("========== UltraMsg ==========");
_logger.LogInformation("URL: {Url}", _httpClient.BaseAddress + relativeUrl);
_logger.LogInformation("To: {To}", request.To);
_logger.LogInformation("Status: {Status}", response.StatusCode);
_logger.LogInformation("Response: {Response}", raw);
_logger.LogInformation("==============================");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "UltraMsg send failed. Status: {Status}, Body: {Body}",
                        response.StatusCode, raw);
                    return WhatsAppMessageResult.Failure(
                        $"UltraMsg returned HTTP {(int)response.StatusCode}", raw);
                }

                string? messageId = null;
                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    // Typical success payload: { "sent":"true", "message":"ok", "id": 123456 }
                    if (doc.RootElement.TryGetProperty("id", out var idElement))
                    {
                        messageId = idElement.ToString();
                    }

                    if (doc.RootElement.TryGetProperty("sent", out var sentElement) &&
                        sentElement.ToString().Equals("false", StringComparison.OrdinalIgnoreCase))
                    {
                        var errorMsg = doc.RootElement.TryGetProperty("message", out var msgElement)
                            ? msgElement.ToString()
                            : "UltraMsg reported the message was not sent";
                        return WhatsAppMessageResult.Failure(errorMsg, raw);
                    }
                }
                catch (JsonException)
                {
                    // Non-JSON response body — treat as success since status code was 2xx,
                    // but keep the raw body for troubleshooting.
                    _logger.LogDebug("UltraMsg response was not JSON: {Body}", raw);
                }

                return WhatsAppMessageResult.Success(messageId, raw);
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling UltraMsg for recipient {To}", request.To);
                return WhatsAppMessageResult.Failure($"Error calling UltraMsg: {ex.Message}");
            }
        }
    }
}
