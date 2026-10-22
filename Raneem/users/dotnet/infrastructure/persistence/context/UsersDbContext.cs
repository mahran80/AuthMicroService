using Microsoft.EntityFrameworkCore;
using Users.Domain.Entities;

namespace Users.Infrastructure.Persistence.Context;

public sealed class UsersDbContext : DbContext
{
    public UsersDbContext(
        DbContextOptions<UsersDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<UserEmail> UserEmails => Set<UserEmail>();

    public DbSet<UserPhone> UserPhones => Set<UserPhone>();
public DbSet<SetPasswordToken> SetPasswordTokens
    => Set<SetPasswordToken>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================
        // User
        // ==========================
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("IAM_USER_User");

            entity.HasKey(x => x.UserId);

            entity.Property(x => x.UserId);
        });

        // ==========================
        // User Email
        // ==========================
        modelBuilder.Entity<UserEmail>(entity =>
        {
            entity.ToTable("IAM_Authentication_Email");

            entity.HasKey(x => x.EmailId);

            entity.Property(x => x.EmailId);

            entity.Property(x => x.UserId)
                  .IsRequired();

            entity.Property(x => x.Email)
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(x => x.IsPrimary)
                  .IsRequired();

            entity.Property(x => x.IsVerified)
                  .IsRequired();

            entity.HasIndex(x => x.Email)
                  .IsUnique();

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ==========================
        // User Phone
        // ==========================
        modelBuilder.Entity<UserPhone>(entity =>
        {
            entity.ToTable("IAM_Authentication_Phone");

            entity.HasKey(x => x.PhoneId);

            entity.Property(x => x.PhoneId);

            entity.Property(x => x.UserId)
                  .IsRequired();

            entity.Property(x => x.CountryCode)
                  .HasMaxLength(5)
                  .IsRequired();
   entity.Property(x => x.Name)
          .HasMaxLength(200);
            entity.Property(x => x.PhoneNumber)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(x => x.IsPrimary)
                  .IsRequired();

            entity.Property(x => x.IsVerified)
                  .IsRequired();

            entity.HasIndex(x => new
            {
                x.CountryCode,
                x.PhoneNumber
            }).IsUnique();

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
//    modelBuilder.Entity<PasswordResetToken>(entity =>
// {
//     entity.ToTable("IAM_Authentication_PasswordResetToken");

//     entity.HasKey(x => x.PasswordResetTokenId);

//     entity.Property(x => x.PasswordResetTokenId);

//     entity.Property(x => x.UserId)
//           .IsRequired();

//     entity.Property(x => x.Token)
//           .HasMaxLength(255)
//           .IsRequired();

//     entity.Property(x => x.CreatedAt)
//           .IsRequired();

//     entity.Property(x => x.ExpireAt)
//           .IsRequired();

//     entity.Property(x => x.IsUsed)
//           .IsRequired();

//     entity.HasOne<User>()
//           .WithMany()
//           .HasForeignKey(x => x.UserId)
//           .OnDelete(DeleteBehavior.Cascade);
// });
   modelBuilder.Entity<SetPasswordToken>(entity =>
{
    entity.ToTable("IAM_Authentication_SetPasswordToken");

    entity.HasKey(x => x.SetPasswordTokenId);

    entity.Property(x => x.SetPasswordTokenId);

    entity.Property(x => x.UserId)
          .IsRequired();

    entity.Property(x => x.Token)
          .HasMaxLength(255)
          .IsRequired();

    entity.Property(x => x.CreatedAt)
          .IsRequired();

    entity.Property(x => x.ExpireAt)
          .IsRequired();

    entity.Property(x => x.IsUsed)
          .IsRequired();

    entity.HasOne<User>()
          .WithMany()
          .HasForeignKey(x => x.UserId)
          .OnDelete(DeleteBehavior.Cascade);
}); }
}