namespace Auth.Domain.Entities;

public sealed class Password
{
    public Guid PasswordId { get; set; }

    public Guid UserId { get; set; }

    public string PasswordHash { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }

public DateTime? DeletedAt { get; set; }

    public string PasswordAlgorithm { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}