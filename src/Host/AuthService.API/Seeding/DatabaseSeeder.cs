using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Identity.Application.Abstractions.Ports;
using Identity.Domain.Entities;
using Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace AuthService.API.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        try
        {
            logger.LogInformation("Applying migrations and running database seeding across all 3 modules...");

            var identityContext = services.GetRequiredService<IdentityDbContext>();
            var rbacContext = services.GetRequiredService<AccessControlDbContext>();
            var usersContext = services.GetRequiredService<UsersDbContext>();
            var passwordHasher = services.GetRequiredService<IPasswordHasherPort>();

            // 1. Run migrations across [identity], [rbac], and [users] schemas
            await identityContext.Database.MigrateAsync();
            await rbacContext.Database.MigrateAsync();
            await usersContext.Database.MigrateAsync();

            // 2. Seed Permissions
            var permissionsDefinitions = new (string Code, string DisplayName, string ModuleGroup, string Description)[]
            {
                ("auth:profile:read", "Read Profile", "Identity", "Read own authenticated profile (/api/v1/auth/me)"),
                ("users:read", "View Users", "AccessControl", "View users and their assigned roles/permissions"),
                ("users:manage", "Manage Users", "AccessControl", "Assign roles to users and change user account status"),
                ("roles:read", "View Roles", "AccessControl", "View all roles and the permission catalog"),
                ("roles:write", "Create Roles", "AccessControl", "Create new custom roles"),
                ("roles:manage", "Manage Roles", "AccessControl", "Bind/unbind permissions to roles")
            };

            var permissionMap = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

            foreach (var def in permissionsDefinitions)
            {
                var perm = await rbacContext.Permissions.FirstOrDefaultAsync(p => p.Code == def.Code);
                if (perm is null)
                {
                    perm = Permission.Create(def.Code, def.DisplayName, def.ModuleGroup, def.Description);
                    await rbacContext.Permissions.AddAsync(perm);
                }
                permissionMap[def.Code] = perm;
            }
            await rbacContext.SaveChangesAsync();

            // 3. Seed System Roles & Bind RolePermissions
            var rolesDefinitions = new (string Name, string Description, bool IsSystemRole, string[] Permissions)[]
            {
                (
                    "SuperAdmin",
                    "Complete system administrative access across all modules.",
                    true,
                    new[] { "auth:profile:read", "users:read", "users:manage", "roles:read", "roles:write", "roles:manage" }
                ),
                (
                    "Manager",
                    "Administrative oversight to read users, manage users, read roles, and view profile.",
                    true,
                    new[] { "auth:profile:read", "users:read", "users:manage", "roles:read" }
                ),
                (
                    "StandardUser",
                    "Standard authenticated user with profile reading capability.",
                    true,
                    new[] { "auth:profile:read" }
                )
            };

            var roleMap = new Dictionary<string, Role>(StringComparer.OrdinalIgnoreCase);

            foreach (var rDef in rolesDefinitions)
            {
                string normalized = rDef.Name.ToUpperInvariant();
                var role = await rbacContext.Roles.FirstOrDefaultAsync(r => r.NormalizedName == normalized);
                if (role is null)
                {
                    role = Role.Create(rDef.Name, rDef.Description, rDef.IsSystemRole);
                    await rbacContext.Roles.AddAsync(role);
                    await rbacContext.SaveChangesAsync();
                }
                roleMap[rDef.Name] = role;

                // Bind permissions
                foreach (var permCode in rDef.Permissions)
                {
                    if (permissionMap.TryGetValue(permCode, out var perm))
                    {
                        bool exists = await rbacContext.RolePermissions
                            .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == perm.Id);

                        if (!exists)
                        {
                            var rp = RolePermission.Create(role.Id, perm.Id);
                            await rbacContext.RolePermissions.AddAsync(rp);
                        }
                    }
                }
                await rbacContext.SaveChangesAsync();
            }

            // 4. Seed Default Users in Identity
            // User 1: Super Admin
            const string adminEmail = "admin@authservice.local";
            var adminUser = await identityContext.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser is null)
            {
                var adminHash = passwordHasher.HashPassword("Admin@12345!");
                adminUser = User.Create(
                    adminEmail,
                    adminHash,
                    "System",
                    "Administrator",
                    countryCode: "+20",
                    phoneNumber: "1000000001",
                    isEmailVerified: true);

                adminUser.VerifyPhone();
                await identityContext.Users.AddAsync(adminUser);
                await identityContext.SaveChangesAsync();
                logger.LogInformation("Seeded default SuperAdmin user {Email}", adminEmail);
            }

            if (roleMap.TryGetValue("SuperAdmin", out var superAdminRole))
            {
                bool hasRole = await rbacContext.UserRoles
                    .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == superAdminRole.Id);

                if (!hasRole)
                {
                    await rbacContext.UserRoles.AddAsync(UserRole.Create(adminUser.Id, superAdminRole.Id));
                    await rbacContext.SaveChangesAsync();
                }
            }

            // User 2: Standard User
            const string standardEmail = "user@authservice.local";
            var standardUser = await identityContext.Users.FirstOrDefaultAsync(u => u.Email == standardEmail);
            if (standardUser is null)
            {
                var userHash = passwordHasher.HashPassword("User@12345!");
                standardUser = User.Create(
                    standardEmail,
                    userHash,
                    "Standard",
                    "User",
                    countryCode: "+20",
                    phoneNumber: "1000000002",
                    isEmailVerified: true);

                standardUser.VerifyPhone();
                await identityContext.Users.AddAsync(standardUser);
                await identityContext.SaveChangesAsync();
                logger.LogInformation("Seeded default StandardUser {Email}", standardEmail);
            }

            if (roleMap.TryGetValue("StandardUser", out var standardRole))
            {
                bool hasRole = await rbacContext.UserRoles
                    .AnyAsync(ur => ur.UserId == standardUser.Id && ur.RoleId == standardRole.Id);

                if (!hasRole)
                {
                    await rbacContext.UserRoles.AddAsync(UserRole.Create(standardUser.Id, standardRole.Id));
                    await rbacContext.SaveChangesAsync();
                }
            }

            // 5. Seed synchronized UserProfile entities in UsersDbContext ([users].[UserProfiles])
            var adminProfile = await usersContext.UserProfiles.FirstOrDefaultAsync(p => p.Id == adminUser.Id);
            if (adminProfile is null)
            {
                adminProfile = UserProfile.Create(
                    adminUser.Id,
                    adminEmail,
                    "System Administrator",
                    "+20",
                    "1000000001");

                await usersContext.UserProfiles.AddAsync(adminProfile);
                await usersContext.SaveChangesAsync();
                logger.LogInformation("Seeded UserProfile for SuperAdmin {Email}", adminEmail);
            }

            var userProfile = await usersContext.UserProfiles.FirstOrDefaultAsync(p => p.Id == standardUser.Id);
            if (userProfile is null)
            {
                userProfile = UserProfile.Create(
                    standardUser.Id,
                    standardEmail,
                    "Standard User",
                    "+20",
                    "1000000002");

                await usersContext.UserProfiles.AddAsync(userProfile);
                await usersContext.SaveChangesAsync();
                logger.LogInformation("Seeded UserProfile for StandardUser {Email}", standardEmail);
            }

            logger.LogInformation("Database migration and seeding completed successfully across all 3 modules.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            // Do not crash application startup if DB is offline during build/test environments
        }
    }
}