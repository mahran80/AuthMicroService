using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class PasswordConfiguration : IEntityTypeConfiguration<Password>
{
    public void Configure(EntityTypeBuilder<Password> builder)
    {
        builder.ToTable("IAM_Authentication_Password");

        builder.HasKey(x => x.PasswordId);

        builder.Property(x => x.PasswordHash)
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.PasswordAlgorithm)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.IsActive);
    }
}