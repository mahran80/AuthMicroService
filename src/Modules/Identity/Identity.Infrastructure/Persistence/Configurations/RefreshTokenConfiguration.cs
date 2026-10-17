using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "identity");

        builder.HasKey(r => r.Id).HasName("PK_RefreshTokens");
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.UserId).IsRequired();

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(r => r.Token)
            .IsUnique()
            .HasDatabaseName("UX_RefreshTokens_Token");

        builder.HasIndex(r => r.UserId)
            .HasDatabaseName("IX_RefreshTokens_UserId");

        builder.Property(r => r.JwtId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(r => r.ExpiresAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(r => r.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(r => r.RevokedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.Property(r => r.ReplacedByToken)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Property(r => r.ReasonRevoked)
            .IsRequired(false)
            .HasMaxLength(256);

        builder.Ignore(r => r.IsExpired);
        builder.Ignore(r => r.IsActive);
    }
}
