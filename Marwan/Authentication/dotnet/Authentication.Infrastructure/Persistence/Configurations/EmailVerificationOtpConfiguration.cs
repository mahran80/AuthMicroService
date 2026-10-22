using Authentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Infrastructure.Persistence.Configurations
{
    public class EmailVerificationOtpConfiguration
        : IEntityTypeConfiguration<EmailVerificationOtp>
    {
        public void Configure(EntityTypeBuilder<EmailVerificationOtp> builder)
        {
            builder.ToTable("EmailVerificationOtps", "iam");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.CodeHash)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .IsRequired();

            builder.Property(x => x.FailedAttempts)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
