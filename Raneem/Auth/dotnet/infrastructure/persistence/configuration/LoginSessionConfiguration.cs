using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class LoginSessionConfiguration : IEntityTypeConfiguration<LoginSession>
{
    public void Configure(EntityTypeBuilder<LoginSession> builder)
    {
        builder.ToTable("IAM_Authentication_LoginSession");

        builder.HasKey(x => x.SessionId);

        builder.Property(x => x.DeviceName)
               .HasMaxLength(100);

        builder.Property(x => x.DeviceType)
               .HasMaxLength(50);

        builder.Property(x => x.IPAddress)
               .HasMaxLength(45);

        builder.Property(x => x.UserAgent)
               .HasMaxLength(255);

        builder.Property(x => x.LoginAt);

        builder.Property(x => x.LogoutAt);

        builder.Property(x => x.IsActive);
    }
}