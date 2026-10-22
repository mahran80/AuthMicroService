using Users.Application.Ports.Outbound;
using Users.Infrastructure.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Users.Infrastructure.Services;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailOptions _options;

    public SmtpEmailService(
        IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        var message = new MimeMessage();

        message.From.Add(
            new MailboxAddress(
                _options.DisplayName,
                _options.From));

        message.To.Add(
            MailboxAddress.Parse(to));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.EnableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None);
        Console.WriteLine(_options.Username);
        Console.WriteLine(_options.Password);
        await smtp.AuthenticateAsync(
            _options.Username,
            _options.Password);
Console.WriteLine(htmlBody);

        await smtp.SendAsync(message);

        await smtp.DisconnectAsync(true);
    }
}