using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<ExternalLogin>
{
    public void Configure(EntityTypeBuilder<ExternalLogin> builder)
    {
        builder.ToTable("ExternalLogins", "identity");

        builder.HasKey(e => e.Id).HasName("PK_ExternalLogins");
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.ProviderKey)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.LinkedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.HasIndex(e => new { e.Provider, e.ProviderKey })
            .IsUnique()
            .HasDatabaseName("UX_ExternalLogins_Provider_Key");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_ExternalLogins_UserId");
    }
}
