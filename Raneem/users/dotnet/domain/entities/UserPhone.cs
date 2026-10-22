namespace Users.Domain.Entities;

public sealed class UserPhone
{
    public Guid? PhoneId { get; set; }

    public Guid? UserId { get; set; }

    public string? CountryCode { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; } = string.Empty;

    public bool? IsPrimary { get; set; }
    public string? Name { get; set; }
    public bool? IsVerified { get; set; }
}