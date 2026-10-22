namespace Identity.Infrastructure.Notifications;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "noreply@authservice.com";
    public string FromName { get; set; } = "Auth Service";
    public bool EnableSsl { get; set; } = true;
    public string FrontendResetUrl { get; set; } = "http://localhost:4200/#/set-password";
}
