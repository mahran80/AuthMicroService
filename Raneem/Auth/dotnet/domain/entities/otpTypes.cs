namespace Auth.Domain.Entities;

public sealed class OTPType
{
    public int TypeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}