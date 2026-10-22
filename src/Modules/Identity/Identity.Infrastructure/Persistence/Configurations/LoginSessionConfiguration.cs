using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Infrastructure.Persistence.Configurations;

public sealed class LoginSessionConfiguration : IEntityTypeConfiguration<LoginSession>
{
    public void Configure(EntityTypeBuilder<LoginSession> builder)
    {
        builder.ToTable("LoginSessions", "identity");

        builder.HasKey(s => s.Id).HasName("PK_LoginSessions");
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.RefreshTokenId)
            .IsRequired(false);

        builder.Property(s => s.DeviceName)
            .IsRequired(false)
            .HasMaxLength(100);

        builder.Property(s => s.DeviceType)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(s => s.IpAddress)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(s => s.UserAgent)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(s => s.LoginAtUtc)
            .IsRequired()
            .HasColumnType("datetime2(7)");

        builder.Property(s => s.LogoutAtUtc)
            .IsRequired(false)
            .HasColumnType("datetime2(7)");

        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(s => s.UserId)
            .HasDatabaseName("IX_LoginSessions_UserId");

        builder.HasIndex(s => s.RefreshTokenId)
            .HasDatabaseName("IX_LoginSessions_RefreshTokenId");
    }
}
