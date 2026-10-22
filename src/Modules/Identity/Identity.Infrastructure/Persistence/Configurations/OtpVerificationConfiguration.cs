using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> builder)
    {
        builder.ToTable("OtpVerifications", "identity");

        builder.HasKey(o => o.Id).HasName("PK_OtpVerifications");
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.CountryCode)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(o => o.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(25);

        builder.Property(o => o.Email)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Property(o => o.CodeHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(o => o.Purpose)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(o => o.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(o => o.ExpiresAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(o => o.UsedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.Property(o => o.FailedAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(o => new { o.UserId, o.Purpose })
            .HasDatabaseName("IX_OtpVerifications_UserId_Purpose");

        builder.Ignore(o => o.IsValid);
    }
}
