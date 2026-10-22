using SharedKernel.Application;

namespace Identity.Application.Abstractions.Ports;

public interface IWhatsAppNotificationPort
{
    Task<Result> SendOtpMessageAsync(
        string countryCode,
        string phoneNumber,
        string otpCode,
        CancellationToken cancellationToken = default);
}
