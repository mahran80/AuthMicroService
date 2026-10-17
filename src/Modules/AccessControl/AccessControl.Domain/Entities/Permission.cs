namespace AccessControl.Domain.Entities;

using SharedKernel.Domain;

public sealed class Permission : Entity
{
    public string Code { get; private set; }
    public string DisplayName { get; private set; }
    public string ModuleGroup { get; private set; }
    public string? Description { get; private set; }

    private Permission()
    {
        Code = string.Empty;
        DisplayName = string.Empty;
        ModuleGroup = string.Empty;
    }

    private Permission(
        Guid id,
        string code,
        string displayName,
        string moduleGroup,
        string? description)
        : base(id)
    {
        Code = code;
        DisplayName = displayName;
        ModuleGroup = moduleGroup;
        Description = description;
    }

    public static Permission Create(string code, string displayName, string moduleGroup, string? description = null)
    {
        return new Permission(
            Guid.NewGuid(),
            code.Trim(),
            displayName.Trim(),
            moduleGroup.Trim(),
            description?.Trim());
    }
}
