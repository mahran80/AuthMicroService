namespace Notifications.WhatsApp.Application.Dtos
{
    /// <summary>
    /// Result of an attempt to send a WhatsApp message through UltraMsg.
    /// </summary>
    public class WhatsAppMessageResult
    {
        public bool IsSuccess { get; init; }

        /// <summary>
        /// UltraMsg message id, present when the send call succeeds.
        /// </summary>
        public string? MessageId { get; init; }

        /// <summary>
        /// Raw JSON payload returned by UltraMsg (kept for logging / debugging).
        /// </summary>
        public string? RawResponse { get; init; }

        /// <summary>
        /// Populated when IsSuccess is false.
        /// </summary>
        public string? ErrorMessage { get; init; }

        public static WhatsAppMessageResult Success(string? messageId, string rawResponse) => new()
        {
            IsSuccess = true,
            MessageId = messageId,
            RawResponse = rawResponse
        };

        public static WhatsAppMessageResult Failure(string errorMessage, string? rawResponse = null) => new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            RawResponse = rawResponse
        };
    }
}
