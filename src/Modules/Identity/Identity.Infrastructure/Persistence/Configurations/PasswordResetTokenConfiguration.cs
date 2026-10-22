using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", "identity");

        builder.HasKey(p => p.Id).HasName("PK_PasswordResetTokens");
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.TokenHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.HasIndex(p => p.TokenHash)
            .IsUnique()
            .HasDatabaseName("UX_PasswordResetTokens_TokenHash");

        builder.HasIndex(p => p.UserId)
            .HasDatabaseName("IX_PasswordResetTokens_UserId");

        builder.Property(p => p.IsInviteToken)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(p => p.ExpiresAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(p => p.UsedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.Ignore(p => p.IsValid);
    }
}
