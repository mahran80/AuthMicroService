using SharedKernel.Application;

namespace Identity.Application.Abstractions.Ports;

public interface IEmailNotificationPort
{
    Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string resetLink,
        CancellationToken cancellationToken = default);

    Task<Result> SendEmailOtpAsync(
        string toEmail,
        string fullName,
        string otpCode,
        CancellationToken cancellationToken = default);
}
