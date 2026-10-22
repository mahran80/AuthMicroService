using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles", "users");

        builder.HasKey(u => u.Id).HasName("PK_UserProfiles");
        builder.Property(u => u.Id)
            .HasColumnName("UserId")
            .ValueGeneratedNever();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UX_UserProfiles_Email");

        builder.Property(u => u.CountryCode)
            .IsRequired(false)
            .HasMaxLength(10);

        builder.Property(u => u.PhoneNumber)
            .IsRequired(false)
            .HasMaxLength(30);

        builder.HasIndex(u => new { u.CountryCode, u.PhoneNumber })
            .IsUnique()
            .HasFilter("[PhoneNumber] IS NOT NULL")
            .HasDatabaseName("UX_UserProfiles_Phone");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(u => u.UpdatedAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");
    }
}
