using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class SetPasswordTokenConfiguration
    : IEntityTypeConfiguration<SetPasswordToken>
{
    public void Configure(
        EntityTypeBuilder<SetPasswordToken> builder)
    {
        builder.ToTable("IAM_Authentication_SetPasswordToken");

        builder.HasKey(x => x.SetPasswordTokenId);

        builder.Property(x => x.Token)
               .HasMaxLength(500);

        builder.Property(x => x.IsUsed);

        builder.Property(x => x.CreatedAt);

        builder.Property(x => x.ExpireAt);
    }
}