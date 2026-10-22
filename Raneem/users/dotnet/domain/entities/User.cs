namespace Users.Domain.Entities;

public sealed class User
{
    public Guid UserId { get; set; }
    public bool IsDeleted { get; set; }

public DateTime? DeletedAt { get; set; }
}