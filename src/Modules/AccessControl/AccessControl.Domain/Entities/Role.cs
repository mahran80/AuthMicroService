namespace AccessControl.Domain.Entities;

using SharedKernel.Domain;

public sealed class Role : Entity
{
    private readonly List<RolePermission> _rolePermissions = new();

    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private init; }
    public DateTime CreatedAtUtc { get; private init; }

    public IReadOnlyCollection<RolePermission> RolePermissions => _rolePermissions.AsReadOnly();

    private Role()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    private Role(
        Guid id,
        string name,
        string normalizedName,
        string? description,
        bool isSystemRole,
        DateTime createdAtUtc)
        : base(id)
    {
        Name = name;
        NormalizedName = normalizedName;
        Description = description;
        IsSystemRole = isSystemRole;
        CreatedAtUtc = createdAtUtc;
    }

    public static Role Create(string name, string? description = null, bool isSystemRole = false)
    {
        string trimmedName = name.Trim();
        return new Role(
            Guid.NewGuid(),
            trimmedName,
            trimmedName.ToUpperInvariant(),
            description?.Trim(),
            isSystemRole,
            DateTime.UtcNow);
    }

    public void UpdateDescription(string? description)
    {
        Description = description?.Trim();
    }
}
