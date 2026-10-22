namespace Auth.Domain.Entities;

public sealed class SetPasswordToken
{
    public Guid SetPasswordTokenId { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpireAt { get; set; }

    public bool IsUsed { get; set; }
}