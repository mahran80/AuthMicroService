namespace Auth.Domain.Entities;

public sealed class PendingRegistration
{
    public Guid PendingRegistrationId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? CountryCode { get; set; }

    public string? PhoneNumber { get; set; }

    public string? PasswordHash { get; set; }

    public string ProjectType { get; set; } = string.Empty;

    public string OtpCode { get; set; } = string.Empty;

    public DateTime ExpireAt { get; set; }

    public DateTime CreatedAt { get; set; }
}