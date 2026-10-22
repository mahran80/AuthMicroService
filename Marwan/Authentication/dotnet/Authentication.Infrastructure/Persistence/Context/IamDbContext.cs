using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Authentication.Infrastructure.Persistence.Context
{
    public class IamDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public IamDbContext(DbContextOptions<IamDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(IamDbContext).Assembly);

            builder.HasDefaultSchema("iam");
        }

        //DbSets
        public DbSet<EmailVerificationOtp> EmailVerificationOtps => Set<EmailVerificationOtp>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    }
}
