using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessControl.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions", "rbac");

        builder.HasKey(p => p.Id).HasName("PK_Permissions");
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("UX_Permissions_Code");

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.ModuleGroup)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Description)
            .IsRequired(false)
            .HasMaxLength(500);
    }
}
