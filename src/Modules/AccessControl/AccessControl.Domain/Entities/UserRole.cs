namespace AccessControl.Domain.Entities;

public sealed class UserRole
{
    public Guid UserId { get; private init; }
    public Guid RoleId { get; private init; }
    public DateTime AssignedAtUtc { get; private init; }

    private UserRole()
    {
    }

    private UserRole(Guid userId, Guid roleId, DateTime assignedAtUtc)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedAtUtc = assignedAtUtc;
    }

    public static UserRole Create(Guid userId, Guid roleId, DateTime? assignedAtUtc = null)
    {
        return new UserRole(userId, roleId, assignedAtUtc ?? DateTime.UtcNow);
    }
}
