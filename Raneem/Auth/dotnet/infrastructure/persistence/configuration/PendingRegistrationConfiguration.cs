using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Persistence.Configurations;

public sealed class PendingRegistrationConfiguration 
    : IEntityTypeConfiguration<PendingRegistration>
{
    public void Configure(
        EntityTypeBuilder<PendingRegistration> builder)
    {
        builder.ToTable(
            "IAM_Authentication_PendingRegistration");


        builder.HasKey(x =>
            x.PendingRegistrationId);


        builder.Property(x =>
            x.Name)
            .HasMaxLength(20);


        builder.Property(x =>
            x.Email)
            .HasMaxLength(255);


        builder.Property(x =>
            x.CountryCode)
            .HasMaxLength(5);


        builder.Property(x =>
            x.PhoneNumber)
            .HasMaxLength(20);


        builder.Property(x =>
            x.OtpCode)
            .HasMaxLength(10)
            .IsRequired();


        builder.Property(x =>
            x.ExpireAt)
            .IsRequired();


        builder.Property(x =>
            x.CreatedAt)
            .IsRequired();
    }
}