namespace Auth.Domain.Entities;

public sealed class Otp
{
    public Guid OtpId { get; set; }

    public Guid UserId { get; set; }

    public string Code { get; set; } = null!;

    public int TypeId { get; set; }

    public string? ProjectType { get; set; } = null!;

    public DateTime ExpireAt { get; set; }

    public bool CanResetPassword { get; set; }

    public bool IsUsed { get; set; }
}