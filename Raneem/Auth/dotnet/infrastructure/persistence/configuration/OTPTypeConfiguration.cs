using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class OTPTypeConfiguration : IEntityTypeConfiguration<OTPType>
{
    public void Configure(EntityTypeBuilder<OTPType> builder)
    {
        builder.ToTable("IAM_Authentication_OTPType");

        builder.HasKey(x => x.TypeId);

        builder.Property(x => x.Name)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(x => x.Description)
               .HasMaxLength(200);
    }
}