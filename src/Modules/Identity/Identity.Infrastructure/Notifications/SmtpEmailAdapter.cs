using Identity.Application.Abstractions.Ports;
using Identity.Domain.Errors;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SharedKernel.Application;

namespace Identity.Infrastructure.Notifications;

public sealed class SmtpEmailAdapter : IEmailNotificationPort
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailAdapter> _logger;

    public SmtpEmailAdapter(
        IOptions<SmtpOptions> options,
        ILogger<SmtpEmailAdapter> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result> SendPasswordResetEmailAsync(
        string toEmail,
        string fullName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        string subject = "Reset Your Password";
        string htmlBody = $"""
            <h2>Password Reset Request</h2>
            <p>Hello {fullName},</p>
            <p>We received a request to reset your password. Click the link below to set a new password:</p>
            <p><a href="{resetLink}" style="padding: 10px 18px; background-color: #2563eb; color: #ffffff; text-decoration: none; border-radius: 4px; display: inline-block;">Reset Password</a></p>
            <p>If you did not request this change, please ignore this email.</p>
            <p>This link is valid for 15 minutes.</p>
            """;

        return await SendEmailInternalAsync(toEmail, subject, htmlBody, cancellationToken);
    }

    public async Task<Result> SendEmailOtpAsync(
        string toEmail,
        string fullName,
        string otpCode,
        CancellationToken cancellationToken = default)
    {
        string subject = "Your Verification Code";
        string htmlBody = $"""
            <h2>Verification Code</h2>
            <p>Hello {fullName},</p>
            <p>Your verification code is: <strong style="font-size: 20px; letter-spacing: 2px;">{otpCode}</strong></p>
            <p>This code will expire in 5 minutes.</p>
            <p>If you did not request this code, please ignore this email.</p>
            """;

        return await SendEmailInternalAsync(toEmail, subject, htmlBody, cancellationToken);
    }

    private async Task<Result> SendEmailInternalAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.Host) || string.IsNullOrWhiteSpace(_options.Username))
        {
            _logger.LogWarning(
                "SMTP Host/Username are not configured. Skipping email delivery to {Email} for local development.",
                toEmail);

            return Result.Success();
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new BodyBuilder
            {
                HtmlBody = htmlBody
            }.ToMessageBody();

            using var smtp = new SmtpClient();

            var secureSocketOption = _options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtp.ConnectAsync(_options.Host, _options.Port, secureSocketOption, cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await smtp.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            }

            await smtp.SendAsync(message, cancellationToken);
            await smtp.DisconnectAsync(true, cancellationToken);

            return Result.Success();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            return Result.Failure(IdentityErrors.NotificationDeliveryFailed);
        }
    }
}
