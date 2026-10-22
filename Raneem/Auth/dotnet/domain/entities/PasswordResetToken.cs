namespace Auth.Domain.Entities;

public sealed class PasswordResetToken
{
    public Guid PasswordResetTokenId { get; set; }

    public Guid UserId { get; set; }

    public string Token { get; set; } = string.Empty;

    public DateTime ExpireAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; }
}