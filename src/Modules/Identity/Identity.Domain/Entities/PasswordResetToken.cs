using SharedKernel.Domain;

namespace Identity.Domain.Entities;

public sealed class PasswordResetToken : Entity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public bool IsInviteToken { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? UsedAtUtc { get; private set; }

    public bool IsValid => UsedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;

    private PasswordResetToken()
    {
        TokenHash = string.Empty;
    }

    private PasswordResetToken(
        Guid id,
        Guid userId,
        string tokenHash,
        bool isInviteToken,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        IsInviteToken = isInviteToken;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static PasswordResetToken Create(
        Guid userId,
        string tokenHash,
        bool isInviteToken,
        TimeSpan? lifetime = null)
    {
        var now = DateTime.UtcNow;
        var expiry = now.Add(lifetime ?? (isInviteToken ? TimeSpan.FromHours(24) : TimeSpan.FromMinutes(15)));

        return new PasswordResetToken(
            Guid.NewGuid(),
            userId,
            tokenHash,
            isInviteToken,
            expiry,
            now);
    }

    public void MarkAsUsed()
    {
        UsedAtUtc = DateTime.UtcNow;
    }
}
