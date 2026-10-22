namespace Users.Domain.Entities;

public sealed class UserEmail
{
    public Guid? EmailId { get; set; }

    public Guid? UserId { get; set; }

    public string? Email { get; set; } = string.Empty;

    public bool? IsPrimary { get; set; }

    public bool? IsVerified { get; set; }
}