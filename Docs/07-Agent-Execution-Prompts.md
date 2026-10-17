# AI Agent Step-by-Step Execution Prompts (Optimized for Small/Weak Models)

**How to use this file:**  
Copy **ONE prompt at a time** and send it to the AI Agent. Wait for the agent to complete the task and pass `dotnet build` before moving to the next prompt.

---

## 🟢 PROMPT 1: Execute Phase 1 (Solution Scaffolding & Packages)

```text
You are implementing Phase 1 of our project. 
Before doing anything, read `docs/02-Architecture-and-Hexagonal-Design.md` and `docs/06-Master-Implementation-Plan.md` (Phase 1 section).

STRICT RULES:
1. Execute ONLY Phase 1. Do NOT write any domain entities, controllers, or business classes yet.
2. Use .NET 8 (`net8.0`) for all projects.
3. Run terminal commands to create the exact solution and projects layout:
   - Solution: `AuthMicroservice.sln` in the root folder.
   - `src/Shared/SharedKernel/SharedKernel.csproj` (classlib)
   - `src/Modules/Identity/Identity.Domain/Identity.Domain.csproj` (classlib)
   - `src/Modules/Identity/Identity.Application/Identity.Application.csproj` (classlib)
   - `src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj` (classlib)
   - `src/Modules/AccessControl/AccessControl.Domain/AccessControl.Domain.csproj` (classlib)
   - `src/Modules/AccessControl/AccessControl.Application/AccessControl.Application.csproj` (classlib)
   - `src/Modules/AccessControl/AccessControl.Infrastructure/AccessControl.Infrastructure.csproj` (classlib)
   - `src/Host/AuthService.API/AuthService.API.csproj` (webapi)
4. Add all projects to `AuthMicroservice.sln`.
5. Configure exact Project References:
   - `Identity.Domain` references ONLY `SharedKernel`.
   - `Identity.Application` references `Identity.Domain` and `SharedKernel`.
   - `Identity.Infrastructure` references `Identity.Application` and `SharedKernel`.
   - `AccessControl.Domain` references ONLY `SharedKernel`.
   - `AccessControl.Application` references `AccessControl.Domain` and `SharedKernel`.
   - `AccessControl.Infrastructure` references `AccessControl.Application` and `SharedKernel`.
   - `AuthService.API` references `SharedKernel`, `Identity.Application`, `Identity.Infrastructure`, `AccessControl.Application`, and `AccessControl.Infrastructure`.
6. Install NuGet packages:
   - In `SharedKernel`: `MediatR` (12.4.1), `FluentValidation` (11.10.0).
   - In `Identity.Application` & `AccessControl.Application`: `FluentValidation.DependencyInjectionExtensions` (11.10.0).
   - In `Identity.Infrastructure`: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.10), `BCrypt.Net-Next` (4.0.3), `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.10).
   - In `AccessControl.Infrastructure`: `Microsoft.EntityFrameworkCore.SqlServer` (8.0.10).
   - In `AuthService.API`: `Swashbuckle.AspNetCore` (6.8.1), `Microsoft.EntityFrameworkCore.Design` (8.0.10), `Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (8.0.10).
7. Delete all default `Class1.cs` and `WeatherForecast.cs` files.
8. Run `dotnet build AuthMicroservice.sln` and show me the output. Stop immediately after `dotnet build` succeeds.
```

---

## 🟢 PROMPT 2: Execute Phase 2 (`SharedKernel` Implementation)

```text
You are now implementing Phase 2 (`SharedKernel`).
Read `docs/06-Master-Implementation-Plan.md` (Phase 2) and `docs/02-Architecture-and-Hexagonal-Design.md`.

STRICT RULES:
1. Modify files ONLY inside `src/Shared/SharedKernel/`. Do NOT touch any other project.
2. Create the following exact files under namespace `SharedKernel.*`:
   - `src/Shared/SharedKernel/Domain/Entity.cs`:
     - Abstract class `Entity` with `public Guid Id { get; protected init; }`.
   - `src/Shared/SharedKernel/Application/ErrorType.cs`:
     - Enum `ErrorType { Failure = 0, Validation = 1, NotFound = 2, Conflict = 3, Unauthorized = 4, Forbidden = 5 }`.
   - `src/Shared/SharedKernel/Application/Error.cs`:
     - Record `Error(string Code, string Description, ErrorType Type, IDictionary<string, string[]>? ValidationErrors = null)` with static factory methods (`None`, `Failure`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Validation`).
   - `src/Shared/SharedKernel/Application/Result.cs`:
     - Class `Result` (`IsSuccess`, `IsFailure`, `Error`, `Success()`, `Failure(Error)`) and `Result<TValue> : Result` (`Value`, `Success(TValue)`, `Failure<TValue>(Error)`).
   - `src/Shared/SharedKernel/Application/Messaging/ICommand.cs`:
     - `ICommand : IRequest<Result>` and `ICommand<TResponse> : IRequest<Result<TResponse>>`.
     - `ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>` and `ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>`.
   - `src/Shared/SharedKernel/Application/Messaging/IQuery.cs`:
     - `IQuery<TResponse> : IRequest<Result<TResponse>>` and `IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>`.
   - `src/Shared/SharedKernel/Application/Behaviors/ValidationBehavior.cs`:
     - MediatR `IPipelineBehavior<TRequest, TResponse>` that runs all registered `IValidator<TRequest>`. If validation fails, construct an `Error.Validation(...)` dictionary by property name and return a failed `Result` or `Result<T>` via reflection without throwing exceptions.
   - `src/Shared/SharedKernel/Contracts/UserAccessProfileDto.cs`:
     - `public sealed record UserAccessProfileDto(IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions);`
   - `src/Shared/SharedKernel/Contracts/IRbacAccessPort.cs`:
     - Interface with methods:
       - `Task<UserAccessProfileDto> GetUserAccessProfileAsync(Guid userId, CancellationToken cancellationToken = default);`
       - `Task<Result> AssignDefaultRoleAsync(Guid userId, string defaultRoleName, CancellationToken cancellationToken = default);`
   - `src/Shared/SharedKernel/Contracts/IUserIdentityPort.cs`:
     - Interface with methods:
       - `Task<bool> CheckUserExistsAsync(Guid userId, CancellationToken cancellationToken = default);`
       - `Task<Result<int>> UpdateUserStatusAndRevokeTokensAsync(Guid userId, string newStatus, CancellationToken cancellationToken = default);`
3. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 3: Execute Phase 3 (`Identity.Domain` & `Identity.Application`)

```text
You are now implementing Phase 3 (`Identity.Domain` and `Identity.Application`).
Read `docs/01-PRD-and-Requirements.md`, `docs/03-State-and-Sequence-Diagrams.md`, `docs/04-Database-Schema-and-DDL.md`, and `docs/05-API-Contracts-and-Swagger-Spec.md`.

STRICT RULES:
1. Work ONLY inside `src/Modules/Identity/Identity.Domain` and `src/Modules/Identity/Identity.Application`.
2. In `Identity.Domain`:
   - `Enums/UserStatus.cs`: `Active = 1`, `Suspended = 2`, `Deactivated = 3`.
   - `Entities/User.cs`: Inherits `Entity`. Properties: `Email`, `PasswordHash`, `FirstName`, `LastName`, `Status` (`UserStatus`), `CreatedAtUtc`, `UpdatedAtUtc`, and `IReadOnlyCollection<RefreshToken> RefreshTokens`. Add static `Create(...)`, `Suspend()`, `Activate()`, `Deactivate()`.
   - `Entities/RefreshToken.cs`: Inherits `Entity`. Properties: `UserId`, `Token`, `JwtId`, `ExpiresAtUtc`, `CreatedAtUtc`, `RevokedAtUtc`, `ReplacedByToken`, `ReasonRevoked`. Computed bool `IsExpired => DateTime.UtcNow >= ExpiresAtUtc` and `IsActive => RevokedAtUtc == null && !IsExpired`. Methods: `Create(...)`, `Revoke(string reason, string? replacedByToken = null)`.
   - `Errors/IdentityErrors.cs`: Static `Error` definitions (`EmailAlreadyInUse`, `InvalidCredentials`, `AccountNotActive`, `InvalidRefreshToken`, `UserNotFound`).
3. In `Identity.Application`:
   - `Abstractions/Ports/IUserRepository.cs`:
     - `Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);`
     - `Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);`
     - `Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);`
     - `Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);`
     - `Task AddUserAsync(User user, CancellationToken ct = default);`
     - `Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken ct = default);`
     - `Task<int> RevokeAllActiveUserTokensAsync(Guid userId, string reason, CancellationToken ct = default);`
     - `Task SaveChangesAsync(CancellationToken ct = default);`
   - `Abstractions/Ports/IPasswordHasherPort.cs`: `string HashPassword(string password); bool VerifyPassword(string password, string passwordHash);`
   - `Abstractions/Ports/IJwtProviderPort.cs`: `TokenPairResult GenerateTokenPair(User user, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);`
   - Implement DTOs and CQRS Command/Query handlers matching `docs/05-API-Contracts-and-Swagger-Spec.md` Section 2:
     - `RegisterUserCommand` (validates email & strong password, hashes password, saves user, calls `IRbacAccessPort.AssignDefaultRoleAsync(user.Id, "StandardUser")`).
     - `LoginCommand` (verifies user & active status, verifies password, fetches roles/permissions from `IRbacAccessPort`, generates JWT + RefreshToken, saves RefreshToken).
     - `RefreshTokenCommand` (finds token, checks `IsActive`, revokes old token with reason `"Rotated"`, loads user + RBAC profile, generates and saves new token pair).
     - `RevokeTokenCommand` (finds token, calls `Revoke("UserLogout")`, saves changes).
   - `DependencyInjection.cs`: Extension method `AddIdentityApplication(this IServiceCollection services)` registering MediatR handlers, `ValidationBehavior`, and FluentValidation validators from this assembly.
4. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 4: Execute Phase 4 (`Identity.Infrastructure`)

```text
You are now implementing Phase 4 (`Identity.Infrastructure`).
Read `docs/02-Architecture-and-Hexagonal-Design.md` and `docs/04-Database-Schema-and-DDL.md`.

STRICT RULES:
1. Work ONLY inside `src/Modules/Identity/Identity.Infrastructure`.
2. Implement the following classes:
   - `Persistence/IdentityDbContext.cs`: Inherits `DbContext`. Sets `modelBuilder.HasDefaultSchema("identity")`. Exposes `DbSet<User> Users` and `DbSet<RefreshToken> RefreshTokens`.
   - `Persistence/Configurations/UserConfiguration.cs`: Maps to table `Users` in schema `identity`. Configures column lengths, unique index `UX_Users_Email`, `Status` stored as `byte` (`TINYINT`), and one-to-many relationship with `RefreshTokens` (`OnDelete(DeleteBehavior.Cascade)`).
   - `Persistence/Configurations/RefreshTokenConfiguration.cs`: Maps to table `RefreshTokens` in schema `identity`. Configures unique index `UX_RefreshTokens_Token` and index `IX_RefreshTokens_UserId`.
   - `Persistence/Repositories/UserRepository.cs`: Implements `IUserRepository` using `IdentityDbContext`.
   - `Security/BcryptPasswordHasherAdapter.cs`: Implements `IPasswordHasherPort` using `BCrypt.Net.BCrypt.EnhancedHashPassword(password, 11)` and `EnhancedVerify`.
   - `Security/JwtOptions.cs`: Configuration POCO (`Issuer`, `Audience`, `SecretKey`, `AccessTokenExpirationMinutes` = 15, `RefreshTokenExpirationDays` = 7).
   - `Security/JwtProviderAdapter.cs`: Implements `IJwtProviderPort`. Uses `HmacSha256`. Adds claims: `sub` (UserId), `email`, `name` (`FirstName + " " + LastName`), `jti` (new Guid), multiple `"roles"` claims, and multiple `"permissions"` claims. Generates a 64-byte cryptographic random Base64 string for the RefreshToken.
   - `Integration/UserIdentityAdapter.cs`: Implements `SharedKernel.Contracts.IUserIdentityPort`. Uses `IUserRepository` to check user existence, update `User.Status` (`Active`/`Suspended`/`Deactivated`), and revoke all active tokens when suspended or deactivated.
   - `DependencyInjection.cs`: Extension method `AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration)` registering `IdentityDbContext`, `JwtOptions`, `IUserRepository`, `IPasswordHasherPort`, `IJwtProviderPort`, and `IUserIdentityPort`.
3. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 5: Execute Phase 5 (`AccessControl.Domain` & `AccessControl.Application`)

```text
You are now implementing Phase 5 (`AccessControl.Domain` and `AccessControl.Application`).
Read `docs/01-PRD-and-Requirements.md`, `docs/04-Database-Schema-and-DDL.md`, and `docs/05-API-Contracts-and-Swagger-Spec.md` (Section 3).

STRICT RULES:
1. Work ONLY inside `src/Modules/AccessControl/AccessControl.Domain` and `src/Modules/AccessControl/AccessControl.Application`.
2. In `AccessControl.Domain`:
   - `Entities/Role.cs`: Inherits `Entity`. Properties: `Name`, `NormalizedName` (`Name.Trim().ToUpperInvariant()`), `Description`, `IsSystemRole`, `CreatedAtUtc`, and collection of `RolePermission`.
   - `Entities/Permission.cs`: Inherits `Entity`. Properties: `Code`, `DisplayName`, `ModuleGroup`, `Description`.
   - `Entities/RolePermission.cs`: Properties: `RoleId`, `PermissionId`, `GrantedAtUtc`.
   - `Entities/UserRole.cs`: Properties: `UserId`, `RoleId`, `AssignedAtUtc`.
   - `Errors/RbacErrors.cs`: Static errors (`RoleAlreadyExists`, `RoleNotFound`, `SystemRoleModificationForbidden`, `InvalidPermissionCodes`, `UserNotFound`).
3. In `AccessControl.Application`:
   - `Abstractions/Ports/IRbacRepository.cs`: Methods to query/add Roles, query Permissions by codes, replace RolePermissions, assign UserRole, and get roles/permissions for a `UserId`.
   - Implement DTOs and CQRS Handlers matching `docs/05-API-Contracts-and-Swagger-Spec.md` Section 3:
     - `CreateRoleCommand` & Handler (`POST /api/v1/rbac/roles`).
     - `GetRolesQuery` & Handler (`GET /api/v1/rbac/roles`).
     - `GetPermissionsQuery` & Handler (`GET /api/v1/rbac/permissions`).
     - `UpdateRolePermissionsCommand` & Handler (`PUT /api/v1/rbac/roles/{roleId}/permissions` - blocks removing core permissions if `role.IsSystemRole && role.NormalizedName == "SUPERADMIN"`).
     - `AssignUserRoleCommand` & Handler (`POST /api/v1/rbac/users/{userId}/roles` - verifies user exists via `IUserIdentityPort.CheckUserExistsAsync`).
     - `UpdateUserStatusCommand` & Handler (`PATCH /api/v1/rbac/users/{userId}/status` - calls `IUserIdentityPort.UpdateUserStatusAndRevokeTokensAsync`).
   - `DependencyInjection.cs`: `AddAccessControlApplication(this IServiceCollection services)`.
4. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 6: Execute Phase 6 (`AccessControl.Infrastructure`)

```text
You are now implementing Phase 6 (`AccessControl.Infrastructure`).
Read `docs/02-Architecture-and-Hexagonal-Design.md` and `docs/04-Database-Schema-and-DDL.md`.

STRICT RULES:
1. Work ONLY inside `src/Modules/AccessControl/AccessControl.Infrastructure`.
2. Implement the following classes:
   - `Persistence/AccessControlDbContext.cs`: Inherits `DbContext`. Sets `modelBuilder.HasDefaultSchema("rbac")`. Exposes `DbSet<Role>`, `DbSet<Permission>`, `DbSet<RolePermission>`, `DbSet<UserRole>`.
   - `Persistence/Configurations/`:
     - `RoleConfiguration.cs`: Table `Roles`, unique index `UX_Roles_NormalizedName`.
     - `PermissionConfiguration.cs`: Table `Permissions`, unique index `UX_Permissions_Code`.
     - `RolePermissionConfiguration.cs`: Table `RolePermissions`, composite PK `(RoleId, PermissionId)`, cascade delete FKs to `Role` and `Permission`.
     - `UserRoleConfiguration.cs`: Table `UserRoles`, composite PK `(UserId, RoleId)`, index `IX_UserRoles_UserId`, cascade delete FK to `Role`.
   - `Persistence/Repositories/RbacRepository.cs`: Implements `IRbacRepository` using `AccessControlDbContext`.
   - `Integration/RbacAccessAdapter.cs`: Implements `SharedKernel.Contracts.IRbacAccessPort`.
     - `GetUserAccessProfileAsync`: Queries `UserRoles` -> `Roles` -> `RolePermissions` -> `Permissions` for the given `userId` and returns distinct role names and permission codes.
     - `AssignDefaultRoleAsync`: Finds role by `NormalizedName` (e.g., `"STANDARDUSER"`) and inserts a `UserRole` record if not already assigned.
   - `DependencyInjection.cs`: `AddAccessControlInfrastructure(this IServiceCollection services, IConfiguration configuration)`.
3. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 7: Execute Phase 7 (`AuthService.API` — Host, Policies, Controllers & Swagger)

```text
You are now implementing Phase 7 (`AuthService.API`).
Read `docs/01-PRD-and-Requirements.md` and `docs/05-API-Contracts-and-Swagger-Spec.md`.

STRICT RULES:
1. Work ONLY inside `src/Host/AuthService.API`.
2. Implement Custom RBAC Permission Authorization:
   - `Authorization/HasPermissionAttribute.cs`: Inherits `AuthorizeAttribute`, sets `Policy = permission`.
   - `Authorization/PermissionRequirement.cs`: Implements `IAuthorizationRequirement` holding `string Permission`.
   - `Authorization/PermissionAuthorizationHandler.cs`: Checks if `context.User.Claims` contains any claim with `Type == "permissions"` and `Value == requirement.Permission`.
   - `Authorization/PermissionPolicyProvider.cs`: Inherits `DefaultAuthorizationPolicyProvider` to dynamically build a policy with `PermissionRequirement(policyName)` when a policy is requested.
3. Implement Error Handling & Result Mapping:
   - `Extensions/ResultExtensions.cs`: Extension method `ToProblemDetails(this Result result, HttpContext httpContext)` mapping `ErrorType` (`Validation` -> 400, `Unauthorized` -> 401, `Forbidden` -> 403, `NotFound` -> 404, `Conflict` -> 409, `Failure` -> 500) matching the RFC 7807 format in `docs/05-API-Contracts-and-Swagger-Spec.md` Section 4.
   - `Middleware/GlobalExceptionHandler.cs`: Implements `IExceptionHandler` returning a 500 `ProblemDetails` JSON response.
4. Implement Controllers matching `docs/05-API-Contracts-and-Swagger-Spec.md`:
   - `Controllers/AuthController.cs` (`/api/v1/auth/register`, `login`, `refresh`, `revoke`, `me`). Note: `/me` reads claims directly from `HttpContext.User` and requires `[HasPermission("auth:profile:read")]`.
   - `Controllers/RbacController.cs` (`/api/v1/rbac/roles`, `permissions`, `roles/{roleId}/permissions`, `users/{userId}/roles`, `users/{userId}/status`) decorated with the exact `[HasPermission("...")]` attributes specified in `docs/05`.
5. Implement Swagger & JWT Setup:
   - `Extensions/SwaggerExtensions.cs`: Configures SwaggerGen with OpenAPI `Bearer` scheme so the "Authorize" button in Swagger UI sends `Authorization: Bearer {token}`.
   - Configure `JwtBearer` authentication in `Program.cs` (set `MapInboundClaims = false` so `"sub"`, `"roles"`, and `"permissions"` claim names are preserved exactly).
   - Update `appsettings.json` with a SQL Server connection string (`Server=localhost;Database=AuthMicroserviceDb;Trusted_Connection=True;TrustServerCertificate=True;`) and `JwtOptions`.
6. Run `dotnet build AuthMicroservice.sln` and verify 0 errors. Stop after building.
```

---

## 🟢 PROMPT 8: Execute Phase 8 (EF Migrations, Seeder & Final Polish)

```text
You are now implementing Phase 8 (Database Migrations, Seeding & Final Readiness).
Read `docs/01-PRD-and-Requirements.md` (Section 5: RBAC Permission Matrix & Default Seed Data).

STRICT RULES:
1. Generate EF Core migrations for both DbContexts:
   - Run `dotnet ef migrations add InitialIdentitySchema --project src/Modules/Identity/Identity.Infrastructure --startup-project src/Host/AuthService.API --context IdentityDbContext --output-dir Persistence/Migrations`
   - Run `dotnet ef migrations add InitialRbacSchema --project src/Modules/AccessControl/AccessControl.Infrastructure --startup-project src/Host/AuthService.API --context AccessControlDbContext --output-dir Persistence/Migrations`
2. Create `src/Host/AuthService.API/Seeding/DatabaseSeeder.cs`:
   - On application startup, calls `MigrateAsync()` on both `IdentityDbContext` and `AccessControlDbContext`.
   - Seeds the 6 permissions (`auth:profile:read`, `users:read`, `users:manage`, `roles:read`, `roles:write`, `roles:manage`) if they do not exist.
   - Seeds the 3 system roles (`SuperAdmin`, `Manager`, `StandardUser`) and binds their exact permissions according to the matrix in `docs/01-PRD-and-Requirements.md` Section 5.2.
   - Seeds the 2 default users (`admin@authservice.local` / `Admin@12345!` assigned to `SuperAdmin`, and `user@authservice.local` / `User@12345!` assigned to `StandardUser`) if they do not exist.
   - Wrap seeding inside a `try/catch` with structured logging so startup is resilient.
3. Map health checks endpoint `/health` in `Program.cs`.
4. Create a clean, professional `README.md` in the root directory explaining the Architecture, how to run the project, and the pre-seeded credentials for testing in Swagger UI.
5. Run `dotnet build AuthMicroservice.sln` and ensure 0 errors and 0 warnings.
```
