using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class OTPConfiguration : IEntityTypeConfiguration<Otp>
{
    public void Configure(EntityTypeBuilder<Otp> builder)
    {
        builder.ToTable("IAM_Authentication_OTP");

        builder.HasKey(x => x.OtpId);

        builder.Property(x => x.OtpId);

        builder.Property(x => x.UserId);

        builder.Property(x => x.Code)
               .HasMaxLength(10)
               .IsRequired();

        builder.Property(x => x.TypeId);

        builder.Property(x => x.ProjectType)
               .HasMaxLength(40)
               .IsRequired(false);

        builder.Property(x => x.ExpireAt);

        builder.Property(x => x.CanResetPassword);

        builder.Property(x => x.IsUsed);
    }
}