using System;

namespace Authentication.Domain.Entities
{
    public class EmailVerificationOtp
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CodeHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public int FailedAttempts { get; set; }
        public ApplicationUser User { get; set; } = null!;
    }
}
