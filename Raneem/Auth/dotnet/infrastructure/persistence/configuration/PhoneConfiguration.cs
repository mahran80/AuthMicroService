using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder.ToTable("IAM_Authentication_Phone");

        builder.HasKey(x => x.PhoneId);

        builder.Property(x => x.CountryCode)
               .HasMaxLength(5)
               .IsRequired();
builder.Property(x => x.Name)
            .HasMaxLength(20)
            .IsRequired(false);
        builder.Property(x => x.PhoneNumber)
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(x => x.IsPrimary);

        builder.Property(x => x.IsVerified);
    }
}