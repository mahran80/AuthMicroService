using AccessControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessControl.Infrastructure.Persistence.Configurations;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions", "rbac");

        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId }).HasName("PK_RolePermissions");
        builder.Property(rp => rp.RoleId).IsRequired();
        builder.Property(rp => rp.PermissionId).IsRequired();

        builder.Property(rp => rp.GrantedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.HasOne<Role>()
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .HasConstraintName("FK_RolePermissions_Roles")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(rp => rp.PermissionId)
            .HasConstraintName("FK_RolePermissions_Permissions")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
