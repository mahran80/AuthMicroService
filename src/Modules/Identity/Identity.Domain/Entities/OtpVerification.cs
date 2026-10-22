using Identity.Domain.Enums;
using SharedKernel.Domain;

namespace Identity.Domain.Entities;

public sealed class OtpVerification : Entity
{
    public Guid UserId { get; private set; }
    public string? CountryCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string CodeHash { get; private set; }
    public OtpPurpose Purpose { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? UsedAtUtc { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsValid => UsedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc && FailedAttempts < 5;

    private OtpVerification()
    {
        CodeHash = string.Empty;
    }

    private OtpVerification(
        Guid id,
        Guid userId,
        string? countryCode,
        string? phoneNumber,
        string? email,
        string codeHash,
        OtpPurpose purpose,
        DateTime expiresAtUtc,
        DateTime createdAtUtc)
        : base(id)
    {
        UserId = userId;
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? null : countryCode.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        CodeHash = codeHash;
        Purpose = purpose;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = createdAtUtc;
        FailedAttempts = 0;
    }

    public static OtpVerification Create(
        Guid userId,
        string? countryCode,
        string? phoneNumber,
        string? email,
        string codeHash,
        OtpPurpose purpose,
        TimeSpan? lifetime = null)
    {
        var now = DateTime.UtcNow;
        var expiry = now.Add(lifetime ?? TimeSpan.FromMinutes(5));

        return new OtpVerification(
            Guid.NewGuid(),
            userId,
            countryCode,
            phoneNumber,
            email,
            codeHash,
            purpose,
            expiry,
            now);
    }

    public void MarkAsUsed()
    {
        UsedAtUtc = DateTime.UtcNow;
    }

    public void IncrementFailedAttempts()
    {
        FailedAttempts++;
    }
}
