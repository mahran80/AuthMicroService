using SharedKernel.Domain;

namespace Users.Domain.Entities;

public sealed class UserProfile : Entity
{
    public string Email { get; private set; }
    public string FullName { get; private set; }
    public string? CountryCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private init; }
    public DateTime? UpdatedAtUtc { get; private set; }

    private UserProfile()
    {
        Email = string.Empty;
        FullName = string.Empty;
    }

    private UserProfile(
        Guid userId,
        string email,
        string fullName,
        string? countryCode,
        string? phoneNumber,
        DateTime createdAtUtc)
        : base(userId)
    {
        Email = email;
        FullName = fullName;
        CountryCode = countryCode;
        PhoneNumber = phoneNumber;
        IsDeleted = false;
        CreatedAtUtc = createdAtUtc;
    }

    public static UserProfile Create(
        Guid userId,
        string email,
        string fullName,
        string? countryCode,
        string? phoneNumber)
    {
        return new UserProfile(
            userId,
            email.Trim().ToLowerInvariant(),
            fullName.Trim(),
            string.IsNullOrWhiteSpace(countryCode) ? null : countryCode.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            DateTime.UtcNow);
    }

    public void UpdateDetails(
        string fullName,
        string email,
        string? countryCode,
        string? phoneNumber)
    {
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        CountryCode = string.IsNullOrWhiteSpace(countryCode) ? null : countryCode.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
