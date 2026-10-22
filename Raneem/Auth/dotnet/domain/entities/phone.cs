namespace Auth.Domain.Entities;

public sealed class Phone
{
    public Guid PhoneId { get; set; }

    public Guid UserId { get; set; }

    public string? CountryCode { get; set; } = null!;

    public string? PhoneNumber { get; set; } = null!;
    public string? Name { get; set; } = null!;
    public bool IsPrimary { get; set; }

    public bool IsVerified { get; set; }
    public bool IsDeleted { get; set; }

public DateTime? DeletedAt { get; set; }
}