namespace Auth.Domain.Entities;

public sealed class Email
{
    public Guid EmailId { get; set; }

    public Guid UserId { get; set; }

    public string EmailAddress { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
    public bool IsDeleted { get; set; }

public DateTime? DeletedAt { get; set; }

    public bool IsVerified { get; set; }
}