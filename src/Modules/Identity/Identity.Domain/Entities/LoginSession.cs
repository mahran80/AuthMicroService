using SharedKernel.Domain;

namespace Identity.Domain.Entities;

public sealed class LoginSession : Entity
{
    public Guid UserId { get; private set; }
    public Guid? RefreshTokenId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? DeviceType { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public DateTime LoginAtUtc { get; private init; }
    public DateTime? LogoutAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    private LoginSession()
    {
    }

    private LoginSession(
        Guid id,
        Guid userId,
        Guid? refreshTokenId,
        string? deviceName,
        string? deviceType,
        string? ipAddress,
        string? userAgent,
        DateTime loginAtUtc)
        : base(id)
    {
        UserId = userId;
        RefreshTokenId = refreshTokenId;
        DeviceName = deviceName;
        DeviceType = deviceType;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        LoginAtUtc = loginAtUtc;
        IsActive = true;
    }

    public static LoginSession Create(
        Guid userId,
        Guid? refreshTokenId,
        string? deviceName,
        string? deviceType,
        string? ipAddress,
        string? userAgent)
    {
        return new LoginSession(
            Guid.NewGuid(),
            userId,
            refreshTokenId,
            deviceName,
            deviceType,
            ipAddress,
            userAgent,
            DateTime.UtcNow);
    }

    public void TerminateSession()
    {
        IsActive = false;
        LogoutAtUtc = DateTime.UtcNow;
    }
}
