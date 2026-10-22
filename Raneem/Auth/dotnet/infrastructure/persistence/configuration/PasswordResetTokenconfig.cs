using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class PasswordResetTokenConfiguration
    : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("IAM_Authentication_PasswordResetToken");

        builder.HasKey(x => x.PasswordResetTokenId);

        builder.Property(x => x.Token)
               .HasMaxLength(500);

        builder.Property(x => x.IsUsed);

        builder.Property(x => x.CreatedAt);

        builder.Property(x => x.ExpireAt);
    }
}