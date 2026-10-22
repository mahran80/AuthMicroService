namespace Auth.Domain.Entities;

public sealed class LoginSession
{
    public Guid SessionId { get; set; }

    public Guid UserId { get; set; }

    public Guid? RefreshTokenId { get; set; }

    public string? DeviceName { get; set; }

    public string? DeviceType { get; set; }

    public string? IPAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime LoginAt { get; set; }

    public DateTime? LogoutAt { get; set; }

    public bool IsActive { get; set; }
}