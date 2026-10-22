using System.ComponentModel.DataAnnotations;

namespace Notifications.WhatsApp.Application.Dtos
{
    /// <summary>
    /// Request to send a plain-text WhatsApp chat message via UltraMsg.
    /// </summary>
    public class WhatsAppMessageRequest
    {
        /// <summary>
        /// Recipient phone number in international format (e.g. +14155552671)
        /// or a chat id (e.g. 14155552671@c.us / 14155552671-441234567890@g.us).
        /// </summary>
        [Required]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Message text to send.
        /// </summary>
        [Required]
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Optional: delay in seconds before the message is sent (UltraMsg supports scheduling).
        /// </summary>
        public int? Priority { get; set; }
    }
}
