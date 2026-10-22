using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Persistence.Context;

public sealed class AuthenticationDbContext : DbContext
{
    public AuthenticationDbContext(
        DbContextOptions<AuthenticationDbContext> options)
        : base(options)
    {
    }
public DbSet<PendingRegistration> PendingRegistrations
    => Set<PendingRegistration>();
    public DbSet<Email> Emails => Set<Email>();
public DbSet<PasswordResetToken> PasswordResetTokens
    => Set<PasswordResetToken>();
    public DbSet<Phone> Phones => Set<Phone>();
public DbSet<OTPType> OtpTypes => Set<OTPType>();
    public DbSet<Password> Passwords => Set<Password>();
public DbSet<SetPasswordToken> SetPasswordTokens => Set<SetPasswordToken>();
public DbSet<Otp> Otps => Set<Otp>();

    public DbSet<OTPType> OTPTypes => Set<OTPType>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<LoginSession> LoginSessions => Set<LoginSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuthenticationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}