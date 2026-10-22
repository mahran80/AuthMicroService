using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users", "identity");

        builder.HasKey(u => u.Id).HasName("PK_Users");
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UX_Users_Email");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.CountryCode)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(u => u.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(25);

        builder.Property(u => u.IsEmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.IsPhoneVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(u => new { u.CountryCode, u.PhoneNumber })
            .HasFilter("[CountryCode] IS NOT NULL AND [PhoneNumber] IS NOT NULL")
            .HasDatabaseName("UX_Users_Phone");

        builder.Property(u => u.Status)
            .HasConversion<byte>()
            .HasColumnType("tinyint")
            .IsRequired()
            .HasDefaultValue(UserStatus.Active);

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(u => u.UpdatedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(r => r.UserId)
            .HasConstraintName("FK_RefreshTokens_Users")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
