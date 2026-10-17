# Master Implementation Plan (Step-by-Step Execution Roadmap)

**Target Audience:** AI Coding Agent & Developer  
**Objective:** Build the Enterprise Auth & RBAC Microservice incrementally across 8 strictly isolated phases to guarantee zero compilation errors and 100% architectural compliance.

---

## Global Rules for the AI Agent
1. **Strict Phase Isolation:** Never implement files belonging to a future phase. Complete ONLY the current phase requested by the user.
2. **Read Docs First:** Always cross-reference `docs/01` through `docs/05` before writing code.
3. **Zero External Dependencies in Domain:** `Identity.Domain` and `AccessControl.Domain` must NEVER reference EF Core, MediatR, or ASP.NET Core packages.
4. **Mandatory Verification Gate:** At the end of every phase, run `dotnet build` in the terminal. Do not mark the phase complete until the solution compiles with **0 Errors**.

---

## Phase 1: Solution Scaffolding, Project References & NuGet Packages
**Goal:** Create the physical `.sln` and `.csproj` structure, configure all inter-project references, and install exact NuGet packages. No business code is written in this phase.

### Deliverables Checklist:
- [ ] Create root solution `AuthMicroservice.sln`.
- [ ] Create `src/Shared/SharedKernel/SharedKernel.csproj` (`net8.0`).
- [ ] Create `Identity` Module projects (`net8.0`):
  - `src/Modules/Identity/Identity.Domain/Identity.Domain.csproj`
  - `src/Modules/Identity/Identity.Application/Identity.Application.csproj`
  - `src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj`
- [ ] Create `AccessControl` Module projects (`net8.0`):
  - `src/Modules/AccessControl/AccessControl.Domain/AccessControl.Domain.csproj`
  - `src/Modules/AccessControl/AccessControl.Application/AccessControl.Application.csproj`
  - `src/Modules/AccessControl/AccessControl.Infrastructure/AccessControl.Infrastructure.csproj`
- [ ] Create Host API project (`net8.0`):
  - `src/Host/AuthService.API/AuthService.API.csproj`
- [ ] Configure strict Clean Architecture Project References:
  - `Identity.Domain` -> `SharedKernel`
  - `Identity.Application` -> `Identity.Domain`, `SharedKernel`
  - `Identity.Infrastructure` -> `Identity.Application`, `SharedKernel`
  - `AccessControl.Domain` -> `SharedKernel`
  - `AccessControl.Application` -> `AccessControl.Domain`, `SharedKernel`
  - `AccessControl.Infrastructure` -> `AccessControl.Application`, `SharedKernel`
  - `AuthService.API` -> All `.Application` and `.Infrastructure` projects + `SharedKernel`.
- [ ] Install required NuGet packages (`MediatR`, `FluentValidation`, `Microsoft.EntityFrameworkCore.SqlServer`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore`).
- [ ] Remove auto-generated `Class1.cs` files.
- [ ] **Verification Gate:** Run `dotnet restore` and `dotnet build`.

---

## Phase 2: SharedKernel Implementation (Cross-Cutting Building Blocks)
**Goal:** Implement the base abstractions, `Result<T>` pattern, CQRS interfaces, validation pipeline behavior, and cross-module Hexagonal Port contracts inside `SharedKernel`.

### Deliverables Checklist:
- [ ] `Domain/Entity.cs`: Base generic entity class with `Guid Id`.
- [ ] `Application/Error.cs` & `Application/ErrorType.cs`: Standardized error record (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`).
- [ ] `Application/Result.cs`: `Result` and `Result<TValue>` pattern implementation.
- [ ] `Application/Messaging/ICommand.cs`, `ICommandHandler.cs`, `IQuery.cs`, `IQueryHandler.cs` (wrapping `MediatR.IRequest`).
- [ ] `Application/Behaviors/ValidationBehavior.cs`: MediatR pipeline behavior executing `FluentValidation` validators and returning `Result.Failure` on validation errors.
- [ ] `Contracts/UserAccessProfileDto.cs`: DTO containing `IReadOnlyList<string> Roles` and `IReadOnlyList<string> Permissions`.
- [ ] `Contracts/IRbacAccessPort.cs`: Cross-module Outbound Port (`GetUserAccessProfileAsync`, `AssignDefaultRoleAsync`).
- [ ] `Contracts/IUserIdentityPort.cs`: Cross-module Outbound Port (`CheckUserExistsAsync`, `RevokeAllUserTokensAsync`).
- [ ] **Verification Gate:** Run `dotnet build src/Shared/SharedKernel/SharedKernel.csproj`.

---

## Phase 3: Module 1 — `Identity.Domain` & `Identity.Application` (Core & Use Cases)
**Goal:** Build the pure Domain layer and Application use cases (CQRS + Inbound/Outbound Ports) for Authentication and Token lifecycle management.

### Deliverables Checklist:
- [ ] **Domain Layer (`Identity.Domain`):**
  - `Enums/UserStatus.cs` (`Active = 1`, `Suspended = 2`, `Deactivated = 3`).
  - `Entities/User.cs` (Rich domain model with private setters, factory `Create(...)`, `Suspend()`, `Activate()`).
  - `Entities/RefreshToken.cs` (Properties matching `docs/04-Database-Schema-and-DDL.md`, methods `IsActive`, `Revoke(...)`, `Rotate(...)`).
  - `Errors/IdentityErrors.cs` (Static domain error definitions).
- [ ] **Application Layer (`Identity.Application`):**
  - `Abstractions/Ports/IUserRepository.cs` (Outbound Port for User & RefreshToken persistence).
  - `Abstractions/Ports/IPasswordHasherPort.cs` (`HashPassword`, `VerifyPassword`).
  - `Abstractions/Ports/IJwtProviderPort.cs` (`GenerateTokenPair`).
  - `DTOs/AuthResponseDtos.cs` (`RegisterUserResponse`, `AuthTokenResponse`, `UserContextResponse`, `TokenPairResult`).
  - `Features/Register/RegisterUserCommand.cs`, `RegisterUserValidator.cs`, `RegisterUserCommandHandler.cs`.
  - `Features/Login/LoginCommand.cs`, `LoginValidator.cs`, `LoginCommandHandler.cs`.
  - `Features/Refresh/RefreshTokenCommand.cs`, `RefreshTokenValidator.cs`, `RefreshTokenCommandHandler.cs`.
  - `Features/Revoke/RevokeTokenCommand.cs`, `RevokeTokenCommandHandler.cs`.
  - `DependencyInjection.cs` (`AddIdentityApplication()`).
- [ ] **Verification Gate:** Run `dotnet build src/Modules/Identity/Identity.Application/Identity.Application.csproj`.

---

## Phase 4: Module 1 — `Identity.Infrastructure` (Secondary/Driven Adapters)
**Goal:** Implement EF Core persistence (`[identity]` schema), BCrypt password hashing adapter, JWT token generation adapter, and the `IUserIdentityPort` adapter.

### Deliverables Checklist:
- [ ] `Persistence/IdentityDbContext.cs` (Default schema set to `"identity"`).
- [ ] `Persistence/Configurations/UserConfiguration.cs` (Matches SQL table `[identity].[Users]`).
- [ ] `Persistence/Configurations/RefreshTokenConfiguration.cs` (Matches SQL table `[identity].[RefreshTokens]`).
- [ ] `Persistence/Repositories/UserRepository.cs` (Implements `IUserRepository`).
- [ ] `Security/BcryptPasswordHasherAdapter.cs` (Implements `IPasswordHasherPort` with WorkFactor 11).
- [ ] `Security/JwtOptions.cs` & `Security/JwtProviderAdapter.cs` (Implements `IJwtProviderPort`, embeds `sub`, `email`, `jti`, `roles`, and `permissions` claims).
- [ ] `Integration/UserIdentityAdapter.cs` (Implements `IUserIdentityPort` for cross-module calls).
- [ ] `DependencyInjection.cs` (`AddIdentityInfrastructure(IConfiguration)`).
- [ ] **Verification Gate:** Run `dotnet build src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj`.

---

## Phase 5: Module 2 — `AccessControl.Domain` & `AccessControl.Application` (RBAC Core)
**Goal:** Build the pure Domain entities and Application CQRS handlers for Roles, Permissions, Role-Permission bindings, and User-Role assignments.

### Deliverables Checklist:
- [ ] **Domain Layer (`AccessControl.Domain`):**
  - `Entities/Role.cs` (`Id`, `Name`, `NormalizedName`, `Description`, `IsSystemRole`, `CreatedAtUtc`, navigation to `RolePermissions`).
  - `Entities/Permission.cs` (`Id`, `Code`, `DisplayName`, `ModuleGroup`, `Description`).
  - `Entities/RolePermission.cs` (`RoleId`, `PermissionId`, `GrantedAtUtc`).
  - `Entities/UserRole.cs` (`UserId`, `RoleId`, `AssignedAtUtc`).
  - `Errors/RbacErrors.cs` (Domain errors for duplicate roles, system role modification protection, etc.).
- [ ] **Application Layer (`AccessControl.Application`):**
  - `Abstractions/Ports/IRbacRepository.cs` (Outbound Port for Roles, Permissions, RolePermissions, UserRoles).
  - `DTOs/RbacDtos.cs` (`RoleDto`, `PermissionDto`, `UserRoleAssignmentDto`, `UpdateUserStatusResponse`).
  - `Features/CreateRole/CreateRoleCommand.cs`, `CreateRoleValidator.cs`, `CreateRoleCommandHandler.cs`.
  - `Features/GetRoles/GetRolesQuery.cs`, `GetRolesQueryHandler.cs`.
  - `Features/GetPermissions/GetPermissionsQuery.cs`, `GetPermissionsQueryHandler.cs`.
  - `Features/UpdateRolePermissions/UpdateRolePermissionsCommand.cs`, `UpdateRolePermissionsCommandHandler.cs`.
  - `Features/AssignUserRole/AssignUserRoleCommand.cs`, `AssignUserRoleCommandHandler.cs`.
  - `Features/UpdateUserStatus/UpdateUserStatusCommand.cs`, `UpdateUserStatusCommandHandler.cs`.
  - `DependencyInjection.cs` (`AddAccessControlApplication()`).
- [ ] **Verification Gate:** Run `dotnet build src/Modules/AccessControl/AccessControl.Application/AccessControl.Application.csproj`.

---

## Phase 6: Module 2 — `AccessControl.Infrastructure` (RBAC Driven Adapters)
**Goal:** Implement EF Core persistence (`[rbac]` schema), repository implementation, and the cross-module `IRbacAccessPort` adapter.

### Deliverables Checklist:
- [ ] `Persistence/AccessControlDbContext.cs` (Default schema set to `"rbac"`).
- [ ] `Persistence/Configurations/RoleConfiguration.cs`, `PermissionConfiguration.cs`, `RolePermissionConfiguration.cs`, `UserRoleConfiguration.cs`.
- [ ] `Persistence/Repositories/RbacRepository.cs` (Implements `IRbacRepository`).
- [ ] `Integration/RbacAccessAdapter.cs` (Implements `IRbacAccessPort` from `SharedKernel`).
- [ ] `DependencyInjection.cs` (`AddAccessControlInfrastructure(IConfiguration)`).
- [ ] **Verification Gate:** Run `dotnet build src/Modules/AccessControl/AccessControl.Infrastructure/AccessControl.Infrastructure.csproj`.

---

## Phase 7: Host API (`AuthService.API`) — Controllers, Custom RBAC Policies & Swagger
**Goal:** Implement the Primary/Driving Adapter: REST Controllers, Custom `[HasPermission]` Policy Authorization, Global Exception Handling (`ProblemDetails`), and Swagger UI with JWT Bearer support.

### Deliverables Checklist:
- [ ] `Authorization/HasPermissionAttribute.cs`.
- [ ] `Authorization/PermissionRequirement.cs` & `PermissionAuthorizationHandler.cs` (Reads `"permissions"` claims from JWT).
- [ ] `Authorization/PermissionPolicyProvider.cs` (Dynamic policy registration for permission codes).
- [ ] `Middleware/GlobalExceptionHandler.cs` & `Extensions/ResultExtensions.cs` (Maps `Result<T>` errors and exceptions to RFC 7807 `ProblemDetails`).
- [ ] `Controllers/AuthController.cs` (`/api/v1/auth/register`, `login`, `refresh`, `revoke`, `me`).
- [ ] `Controllers/RbacController.cs` (`/api/v1/rbac/roles`, `permissions`, `roles/{roleId}/permissions`, `users/{userId}/roles`, `users/{userId}/status`).
- [ ] `Extensions/SwaggerExtensions.cs` (Configures OpenAPI Bearer Security Scheme).
- [ ] `appsettings.json` (Connection string + `JwtOptions` configuration + HealthCheck setup).
- [ ] **Verification Gate:** Run `dotnet build` across the entire solution.

---

## Phase 8: EF Core Migrations, Database Seeder & End-to-End Readiness
**Goal:** Generate EF Core Migrations for both `IdentityDbContext` and `AccessControlDbContext`, implement the automated startup `DatabaseSeeder` (seeding permissions, roles, and default `SuperAdmin` / `StandardUser` accounts), and verify startup.

### Deliverables Checklist:
- [ ] Generate EF Core migrations for `IdentityDbContext` (`Migrations/Identity`).
- [ ] Generate EF Core migrations for `AccessControlDbContext` (`Migrations/AccessControl`).
- [ ] `Infrastructure/Seeding/DatabaseSeeder.cs` in `AuthService.API` (Applies migrations and seeds the exact matrix from `docs/01-PRD-and-Requirements.md` Section 5).
- [ ] Wire `DatabaseSeeder` and `/health` endpoint in `Program.cs`.
- [ ] Create root `README.md` with setup instructions and pre-seeded credentials for immediate Swagger testing.
- [ ] **Verification Gate:** Run `dotnet build` and verify zero warnings/errors.
