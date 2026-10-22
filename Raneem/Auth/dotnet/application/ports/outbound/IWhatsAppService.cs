using Notifications.WhatsApp.Application.Dtos;

namespace Auth.Application.Ports.Outbound
{
    /// <summary>
    /// Outbound port for sending WhatsApp messages. Any provider (UltraMsg today,
    /// something else tomorrow) implements this so the rest of the app never
    /// depends on a specific vendor's SDK or HTTP shape.
    /// </summary>
    public interface IWhatsAppService
    {
        Task<WhatsAppMessageResult> SendTextMessageAsync(
            WhatsAppMessageRequest request,
            CancellationToken cancellationToken = default);
    }
}
