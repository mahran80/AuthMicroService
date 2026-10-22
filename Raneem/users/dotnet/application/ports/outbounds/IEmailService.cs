namespace Users.Application.Ports.Outbound;

public interface IEmailService
{
    Task SendAsync(
        string to,
        string subject,
        string htmlBody);
}