namespace Identity.Domain.Entities;

using SharedKernel.Domain;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; private init; }
    public string Token { get; private init; }
    public string JwtId { get; private init; }
    public DateTime ExpiresAtUtc { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
    public bool IsActive => RevokedAtUtc == null && !IsExpired;

    private RefreshToken()
    {
        Token = string.Empty;
        JwtId = string.Empty;
    }

    private RefreshToken(
        Guid id,
        Guid userId,
        string token,
        string jwtId,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        Token = token;
        JwtId = jwtId;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public static RefreshToken Create(
        Guid userId,
        string token,
        string jwtId,
        DateTime expiresAtUtc,
        DateTime? createdAtUtc = null)
    {
        return new RefreshToken(
            Guid.NewGuid(),
            userId,
            token,
            jwtId,
            expiresAtUtc,
            createdAtUtc ?? DateTime.UtcNow);
    }

    public void Revoke(string reason, string? replacedByToken = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}
