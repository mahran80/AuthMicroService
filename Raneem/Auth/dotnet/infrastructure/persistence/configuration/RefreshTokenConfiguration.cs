using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("IAM_Authentication_RefreshToken");

        builder.HasKey(x => x.RefreshTokenId);

        builder.Property(x => x.Token)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.ExpireAt)
               .IsRequired();

        builder.Property(x => x.IsRevoked);

        builder.Property(x => x.RevokedAt);
    }
}