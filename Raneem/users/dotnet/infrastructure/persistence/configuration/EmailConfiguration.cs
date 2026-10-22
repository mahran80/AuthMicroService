using Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Users.Infrastructure.Persistence.Configurations;

public sealed class EmailConfiguration : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("IAM_Authentication_Email");

        builder.HasKey(x => x.EmailId);

        builder.Property(x => x.EmailId);

        builder.Property(x => x.UserId)
               .IsRequired();

        builder.Property(x => x.EmailAddress)
               .HasColumnName("Email")
               .HasMaxLength(255)
               .IsRequired();

        builder.Property(x => x.IsPrimary);

        builder.Property(x => x.IsVerified);
    }
}