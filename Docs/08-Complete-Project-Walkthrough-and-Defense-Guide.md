# Enterprise IAM & Dynamic RBAC Microservice: Technical Walkthrough & Defense Guide

> **Audience:** Senior Software Engineers, Solution Architects, Technical Interviewers, and Engineering Leadership.  
> **Target Framework:** .NET 8 (LTS) / C# 12  
> **Repository:** `AuthMicroservice.sln`  
> **Architectural Standard:** Modular Architecture, Hexagonal (Ports & Adapters), Clean Architecture (Layered Dependency Rule), CQRS with MediatR, and Domain-Driven Design (DDD).

---

## Table of Contents
1. [Executive Summary & Architectural Big Picture](#1-executive-summary--architectural-big-picture)
2. [Complete Technology Stack, Libraries & Tools Justification](#2-complete-technology-stack-libraries--tools-justification)
3. [Complete File-by-File & Folder Structure Reference Guide](#3-complete-file-by-file--folder-structure-reference-guide)
4. [Step-by-Step SDLC Execution Journey (How We Built It)](#4-step-by-step-sdlc-execution-journey-how-we-built-it)
5. [End-to-End Request Execution Flows (Code-Level Trace)](#5-end-to-end-request-execution-flows-code-level-trace)
6. [Live Swagger Demo Script (Step-by-Step Presentation Guide)](#6-live-swagger-demo-script-step-by-step-presentation-guide)
7. [Top Technical Interview Q&A (Defense Cheat Sheet)](#7-top-technical-interview-qa-defense-cheat-sheet)

---

## 1. Executive Summary & Architectural Big Picture

### 1.1 Executive Overview
This microservice is a production-grade, centralized **Identity and Access Management (IAM)** authority. It delivers:
- **Stateless Authentication:** JSON Web Token (JWT) issuance signed via HMAC-SHA256 with 15-minute validity.
- **Stateful Token Lifecycle:** 64-byte cryptographically random refresh tokens with automatic rotation, replay detection, and administrative cascade revocation.
- **Dynamic Role-Based Access Control (RBAC):** Fine-grained permission-based endpoint security evaluated dynamically without database lookups during request execution.
- **Strict Schema Isolation:** Physical database separation under `identity` and `rbac` schemas within a single SQL Server instance, maintaining zero cross-schema database foreign keys.

---

### 1.2 The Three Architectural Paradigms Combined

Our solution harmonizes three industry-standard architectural paradigms:

```mermaid
graph TD
    subgraph DrivingAdapters ["Primary / Driving Adapters (HTTP / UI)"]
        AC["AuthController (/api/v1/auth)"]
        RC["RbacController (/api/v1/rbac)"]
        Swagger["Swagger UI (Bearer Token)"]
    end

    subgraph InboundPorts ["Application Inbound Ports (CQRS Commands & Queries)"]
        CMD_Auth["RegisterUserCommand, LoginCommand, RefreshTokenCommand, RevokeTokenCommand"]
        CMD_Rbac["CreateRoleCommand, UpdateRolePermissionsCommand, AssignUserRoleCommand, UpdateUserStatusCommand"]
        QRY_Rbac["GetRolesQuery, GetPermissionsQuery"]
    end

    subgraph DomainCore ["Domain Core (Pure C# - Zero Dependencies)"]
        IdentityDomain["Identity.Domain: User, RefreshToken, UserStatus, IdentityErrors"]
        RbacDomain["AccessControl.Domain: Role, Permission, RolePermission, UserRole, RbacErrors"]
    end

    subgraph OutboundPorts ["Application Outbound Ports (Abstractions)"]
        IUserRepo["IUserRepository"]
        IPasswordHasher["IPasswordHasherPort"]
        IJwtProvider["IJwtProviderPort"]
        IRbacRepo["IRbacRepository"]
        IRbacAccess["IRbacAccessPort (SharedKernel)"]
        IUserIdentity["IUserIdentityPort (SharedKernel)"]
    end

    subgraph DrivenAdapters ["Secondary / Driven Adapters (Infrastructure)"]
        UserRepoImpl["UserRepository (EF Core)"]
        HasherImpl["BcryptPasswordHasherAdapter (BCrypt.Net-Next)"]
        JwtImpl["JwtProviderAdapter (System.IdentityModel)"]
        RbacRepoImpl["RbacRepository (EF Core)"]
        RbacAdapterImpl["RbacAccessAdapter"]
        IdentityAdapterImpl["UserIdentityAdapter"]
    end

    subgraph DatabaseLayer ["Persistence & External Storage"]
        IdentityDb[("SQL Server: [identity] Schema")]
        RbacDb[("SQL Server: [rbac] Schema")]
    end

    DrivingAdapters -->|Dispatches via MediatR| InboundPorts
    InboundPorts -->|Executes Business Logic| DomainCore
    InboundPorts -->|Calls Outbound Ports| OutboundPorts
    OutboundPorts -.->|Implemented By| DrivenAdapters
    DrivenAdapters -->|Reads/Writes| DatabaseLayer
    IdentityAdapterImpl -.->|Implements| IUserIdentity
    RbacAdapterImpl -.->|Implements| IRbacAccess
    CMD_Auth -.->|Invokes Cross-Module Port| IRbacAccess
    CMD_Rbac -.->|Invokes Cross-Module Port| IUserIdentity
```

#### 1. Modular Architecture (Bounded Contexts)
Instead of building an untamed monolith or jumping prematurely to distributed microservices with network latency and distributed transaction complexities, we created a **Modular Monolith**:
- **Identity Bounded Context:** Owns authentication, password hashing, JWT creation, token rotation, and user lifecycle states (`Active`, `Suspended`, `Deactivated`).
- **AccessControl Bounded Context:** Owns roles, granular permission definitions, role-permission mappings, and user-role assignments.
- **SharedKernel:** Houses shared primitives (`Entity`, `Result<T>`, `Error`, CQRS messaging contracts, validation pipeline behaviors, and cross-module interface contracts).
- **Physical Zero-Dependency Guarantee:** `Identity` projects and `AccessControl` projects have **zero project references to each other**. If either module needs data or functionality from the other, it communicates strictly via contracts in `SharedKernel.Contracts`.

#### 2. Hexagonal Architecture (Ports & Adapters)
- **Primary / Driving Ports:** Application-layer CQRS Command & Query interfaces (`ICommand<T>`, `IQuery<T>`).
- **Primary / Driving Adapters:** ASP.NET Core REST API Controllers (`AuthController`, `RbacController`) and Swagger UI.
- **Secondary / Driven Ports:** Application interfaces defining what the domain/application requires without knowing how it is implemented (`IUserRepository`, `IPasswordHasherPort`, `IJwtProviderPort`, `IRbacRepository`, `IRbacAccessPort`, `IUserIdentityPort`).
- **Secondary / Driven Adapters:** Infrastructure implementations (`UserRepository`, `BcryptPasswordHasherAdapter`, `JwtProviderAdapter`, `RbacRepository`, `RbacAccessAdapter`, `UserIdentityAdapter`).

#### 3. Clean Architecture (Layered Dependency Rule)
Dependencies strictly flow **inward**:
```
API Host / Infrastructure ---> Application Layer ---> Domain Core
```
- **Domain Layer (`.Domain`):** Pure C# entities with encapsulated invariants. Has **zero** NuGet package dependencies and **zero** references to EF Core, ASP.NET Core, or MediatR.
- **Application Layer (`.Application`):** Contains CQRS business use cases, DTOs, FluentValidation validators, and Port abstractions. It references only `.Domain` and `SharedKernel`.
- **Infrastructure Layer (`.Infrastructure`):** Implements ports using EF Core, SQL Server, BCrypt, and System.IdentityModel. References `.Application` and `SharedKernel`.
- **Host Layer (`AuthService.API`):** Acts as the composition root, bootstrapping the IoC container, configuring middleware, and mapping endpoints.

---
## 2. Complete Technology Stack, Libraries & Tools Justification

The following comprehensive table outlines every framework, library, and architectural pattern utilized across the solution, where it is used, and the explicit technical rationale for selecting it over common industry alternatives:

| Technology / Library | Version | Usage Layer | Architectural Purpose & Rationale | Alternatives Considered & Why Rejected |
| :--- | :---: | :--- | :--- | :--- |
| **.NET 8 (C# 12)** | `8.0` (LTS) | Entire Solution | Long-Term Support release offering top-tier performance, primitive collections improvements, UTF-8 string literals, primary constructors, and native AOT readiness. | .NET 7 (STS, reached end of life); .NET 6 (older LTS without C# 12 and performance improvements). |
| **EF Core 8** | `8.0.10` | Infrastructure & API | High-productivity Code-First ORM supporting schema separation (`identity` and `rbac`), migrations history isolation, change tracking, and strongly-typed queries. | **Dapper / Raw ADO.NET:** Lacks automated migrations, relational change tracking, and model navigation properties needed for complex aggregate persistence. |
| **SQL Server Schemas** | Latest | Database Server | Logical partitioning into `[identity]` and `[rbac]` schemas within a single physical database. Enforces zero cross-schema foreign keys while enabling unified backup and transactional consistency. | **Multiple Databases:** Overkill at this stage, introduces distributed transaction (2PC / Saga) overhead. **Single dbo Schema:** Causes table name collisions, leaks bounded contexts, and breaks domain isolation. |
| **MediatR** | `12.4.1` | Application & API | In-process messaging mediator implementing the CQRS (Command Query Responsibility Segregation) pattern. Decouples HTTP controllers from business application handlers. | **Direct Service Classes:** Leads to bloated God-services (e.g. `IUserService` with 25 methods), circular DI dependencies, and tight coupling. |
| **FluentValidation** | `11.10.0` | Application & SharedKernel | Strongly-typed, declarative validation rules executed automatically through MediatR pipeline behaviors (`ValidationBehavior<,>`) prior to entering business handlers. | **Data Annotations:** Clutters commands/DTOs with attributes, difficult to test, limited to basic validations, lacks rich rule chaining and cross-property validation. |
| **BCrypt.Net-Next** | `4.0.3` | Identity.Infrastructure | Cryptographic password hashing utilizing an adaptive cost factor (Work Factor = 12) with embedded salt and defense against timing attacks (`EnhancedVerify`). | **PBKDF2 / SHA256:** Vulnerable to specialized GPU/ASIC brute-force hardware. **Argon2:** Excellent, but BCrypt is battle-tested, widely supported across platforms, and simpler to configure. |
| **JwtBearer Auth** | `8.0.10` | AuthService.API | Official ASP.NET Core middleware providing stateless JWT bearer authentication with fine-tuned claims mapping (`MapInboundClaims = false`). | **Cookies / Session:** Stateful, requires sticky sessions or distributed Redis cache, not suitable for cross-origin SPA, mobile, and microservice consumption. |
| **Swashbuckle / OpenAPI** | `6.8.1` | AuthService.API | Generates OpenAPI 3.0 specifications and interactive Swagger UI configured with Bearer security definitions for instant manual verification. | **NSwag:** Good, but Swashbuckle is standard, widely adopted, and features seamless custom filter/security extensions. |
| **Custom Domain Entities** | Custom | Domain Layers | Pure DDD entities (`User`, `Role`, `Permission`) with private parameterless constructors, private setters, and static factory methods enforcing encapsulation. | **ASP.NET Core Identity:** Heavily opinionated, leaks framework concerns (`IdentityUser<string>`) into domain, forces 8 redundant tables, and breaks Hexagonal Architecture. |
| **Result<T> Pattern** | Custom | SharedKernel | Railway-oriented programming pattern returning explicit Success/Failure outcomes with strongly-typed `Error` records without throwing expensive exceptions. | **Throwing Exceptions for Flow Control:** Degrades CPU performance (stack trace generation), creates unpredictable control flows, and leads to unhandled runtime crashes. |
| **RFC 7807 ProblemDetails**| Standard | AuthService.API | Standardized REST error response format (`application/problem+json`) containing `type`, `title`, `status`, `detail`, `instance`, `errorCode`, and `errors`. | **Arbitrary JSON Error Objects:** Inconsistent error responses across endpoints create integration friction for client applications. |
| **Dynamic Policy Provider** | Custom | AuthService.API | `PermissionPolicyProvider` dynamically constructs authorization policies from permission strings on-the-fly without static policy registration. | **Static Policies / Roles (`[Authorize(Roles = "...")]`):** Inflexible, requires code changes and deployments to introduce new permissions, fails granular RBAC requirements. |

---
## 3. Complete File-by-File & Folder Structure Reference Guide

### 3.1 Solution Physical Directory Tree

```
AuthMicroservice/
│
├── AuthMicroservice.sln
├── README.md
│
├── src/
│   ├── Shared/
│   │   └── SharedKernel/
│   │       ├── Domain/
│   │       │   └── Entity.cs
│   │       ├── Application/
│   │       │   ├── ErrorType.cs
│   │       │   ├── Error.cs
│   │       │   ├── Result.cs
│   │       │   ├── Messaging/
│   │       │   │   ├── ICommand.cs
│   │       │   │   └── IQuery.cs
│   │       │   └── Behaviors/
│   │       │       └── ValidationBehavior.cs
│   │       ├── Contracts/
│   │       │   ├── UserAccessProfileDto.cs
│   │       │   ├── IRbacAccessPort.cs
│   │       │   └── IUserIdentityPort.cs
│   │       └── SharedKernel.csproj
│   │
│   ├── Modules/
│   │   ├── Identity/
│   │   │   ├── Identity.Domain/
│   │   │   │   ├── Entities/
│   │   │   │   │   ├── User.cs
│   │   │   │   │   └── RefreshToken.cs
│   │   │   │   ├── Enums/
│   │   │   │   │   └── UserStatus.cs
│   │   │   │   ├── Errors/
│   │   │   │   │   └── IdentityErrors.cs
│   │   │   │   └── Identity.Domain.csproj
│   │   │   │
│   │   │   ├── Identity.Application/
│   │   │   │   ├── Abstractions/
│   │   │   │   │   └── Ports/
│   │   │   │   │       ├── IUserRepository.cs
│   │   │   │   │       ├── IPasswordHasherPort.cs
│   │   │   │   │       └── IJwtProviderPort.cs
│   │   │   │   ├── DTOs/
│   │   │   │   │   └── AuthResponseDtos.cs
│   │   │   │   ├── Features/
│   │   │   │   │   ├── Register/
│   │   │   │   │   │   ├── RegisterUserCommand.cs
│   │   │   │   │   │   ├── RegisterUserCommandHandler.cs
│   │   │   │   │   │   └── RegisterUserValidator.cs
│   │   │   │   │   ├── Login/
│   │   │   │   │   │   ├── LoginCommand.cs
│   │   │   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   │   │   └── LoginValidator.cs
│   │   │   │   │   ├── Refresh/
│   │   │   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   │   │   ├── RefreshTokenCommandHandler.cs
│   │   │   │   │   │   └── RefreshTokenValidator.cs
│   │   │   │   │   └── Revoke/
│   │   │   │   │       ├── RevokeTokenCommand.cs
│   │   │   │   │       ├── RevokeTokenCommandHandler.cs
│   │   │   │   │       └── RevokeTokenValidator.cs
│   │   │   │   ├── DependencyInjection.cs
│   │   │   │   └── Identity.Application.csproj
│   │   │   │
│   │   │   └── Identity.Infrastructure/
│   │   │       ├── Persistence/
│   │   │       │   ├── IdentityDbContext.cs
│   │   │       │   ├── Configurations/
│   │   │       │   │   ├── UserConfiguration.cs
│   │   │       │   │   └── RefreshTokenConfiguration.cs
│   │   │       │   ├── Repositories/
│   │   │       │   │   └── UserRepository.cs
│   │   │       │   └── Migrations/
│   │   │       │       └── 20261017121316_InitialIdentitySchema.cs
│   │   │       ├── Security/
│   │   │       │   ├── JwtOptions.cs
│   │   │       │   ├── JwtProviderAdapter.cs
│   │   │       │   └── BcryptPasswordHasherAdapter.cs
│   │   │       ├── Integration/
│   │   │       │   └── UserIdentityAdapter.cs
│   │   │       ├── DependencyInjection.cs
│   │   │       └── Identity.Infrastructure.csproj
│   │   │
│   │   └── AccessControl/
│   │       ├── AccessControl.Domain/
│   │       │   ├── Entities/
│   │       │   │   ├── Role.cs
│   │       │   │   ├── Permission.cs
│   │       │   │   ├── RolePermission.cs
│   │       │   │   └── UserRole.cs
│   │       │   ├── Errors/
│   │       │   │   └── RbacErrors.cs
│   │       │   └── AccessControl.Domain.csproj
│   │       │
│   │       ├── AccessControl.Application/
│   │       │   ├── Abstractions/
│   │       │   │   └── Ports/
│   │       │   │       └── IRbacRepository.cs
│   │       │   ├── DTOs/
│   │       │   │   └── RbacDtos.cs
│   │       │   ├── Features/
│   │       │   │   ├── CreateRole/
│   │       │   │   │   ├── CreateRoleCommand.cs
│   │       │   │   │   ├── CreateRoleCommandHandler.cs
│   │       │   │   │   └── CreateRoleValidator.cs
│   │       │   │   ├── GetRoles/
│   │       │   │   │   ├── GetRolesQuery.cs
│   │       │   │   │   └── GetRolesQueryHandler.cs
│   │       │   │   ├── GetPermissions/
│   │       │   │   │   ├── GetPermissionsQuery.cs
│   │       │   │   │   └── GetPermissionsQueryHandler.cs
│   │       │   │   ├── UpdateRolePermissions/
│   │       │   │   │   ├── UpdateRolePermissionsCommand.cs
│   │       │   │   │   ├── UpdateRolePermissionsCommandHandler.cs
│   │       │   │   │   └── UpdateRolePermissionsValidator.cs
│   │       │   │   ├── AssignUserRole/
│   │       │   │   │   ├── AssignUserRoleCommand.cs
│   │       │   │   │   ├── AssignUserRoleCommandHandler.cs
│   │       │   │   │   └── AssignUserRoleValidator.cs
│   │       │   │   └── UpdateUserStatus/
│   │       │   │       ├── UpdateUserStatusCommand.cs
│   │       │   │       ├── UpdateUserStatusCommandHandler.cs
│   │       │   │       └── UpdateUserStatusValidator.cs
│   │       │   ├── DependencyInjection.cs
│   │       │   └── AccessControl.Application.csproj
│   │       │
│   │       └── AccessControl.Infrastructure/
│   │           ├── Persistence/
│   │           │   ├── AccessControlDbContext.cs
│   │           │   ├── Configurations/
│   │           │   │   ├── RoleConfiguration.cs
│   │           │   │   ├── PermissionConfiguration.cs
│   │           │   │   ├── RolePermissionConfiguration.cs
│   │           │   │   └── UserRoleConfiguration.cs
│   │           │   ├── Repositories/
│   │           │   │   └── RbacRepository.cs
│   │           │   └── Migrations/
│   │           │       └── 20261017121419_InitialRbacSchema.cs
│   │           ├── Integration/
│   │           │   └── RbacAccessAdapter.cs
│   │           ├── DependencyInjection.cs
│   │           └── AccessControl.Infrastructure.csproj
│   │
│   └── Host/
│       └── AuthService.API/
│           ├── Authorization/
│           │   ├── HasPermissionAttribute.cs
│           │   ├── PermissionRequirement.cs
│           │   ├── PermissionAuthorizationHandler.cs
│           │   └── PermissionPolicyProvider.cs
│           ├── Contracts/
│           │   ├── AuthRequests.cs
│           │   └── RbacRequests.cs
│           ├── Controllers/
│           │   ├── AuthController.cs
│           │   └── RbacController.cs
│           ├── Extensions/
│           │   ├── ResultExtensions.cs
│           │   └── SwaggerExtensions.cs
│           ├── Middleware/
│           │   └── GlobalExceptionHandler.cs
│           ├── Seeding/
│           │   └── DatabaseSeeder.cs
│           ├── appsettings.json
│           ├── appsettings.Development.json
│           ├── Program.cs
│           └── AuthService.API.csproj
```

---

### 3.2 Detailed File Responsibilities by Project

#### 1. `src/Shared/SharedKernel`
- `Domain/Entity.cs`: Abstract base entity providing strongly typed `Guid Id` with protected initialization.
- `Application/ErrorType.cs`: Enumeration (`Failure`, `Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`) driving HTTP status code mapping.
- `Application/Error.cs`: Immutable record holding `Code`, `Description`, `ErrorType`, and validation errors dictionary with static factory methods.
- `Application/Result.cs`: Functional result pattern encapsulating `IsSuccess`, `IsFailure`, and `Error`, plus generic `Result<TValue>` carrying payload data.
- `Application/Messaging/ICommand.cs` & `IQuery.cs`: Strongly typed wrappers over MediatR `IRequest<Result>` and `IRequest<Result<TResponse>>` establishing CQRS segregation.
- `Application/Behaviors/ValidationBehavior.cs`: MediatR open pipeline behavior scanning `IEnumerable<IValidator<TRequest>>` and short-circuiting pipeline with `Result.Failure` on validation violation.
- `Contracts/UserAccessProfileDto.cs`: Cross-boundary DTO transferring user role names and permission codes.
- `Contracts/IRbacAccessPort.cs`: Outbound contract enabling Identity to query access profiles and assign default roles without referencing RBAC entities.
- `Contracts/IUserIdentityPort.cs`: Outbound contract enabling AccessControl to query user existence and revoke refresh tokens without referencing Identity entities.

#### 2. `src/Modules/Identity/Identity.Domain`
- `Entities/User.cs`: Rich domain aggregate root encapsulating email, BCrypt password hash, names, `UserStatus`, audit timestamps, and collection of refresh tokens. Exposes lifecycle methods (`Suspend()`, `Activate()`, `Deactivate()`, `AddRefreshToken()`).
- `Entities/RefreshToken.cs`: State-aware entity tracking token string, JWT ID (`jti`), expiration, revocation timestamp, replaced token reference, and audit reason.
- `Enums/UserStatus.cs`: `Active = 1`, `Suspended = 2`, `Deactivated = 3`.
- `Errors/IdentityErrors.cs`: Domain error catalog (`EmailAlreadyInUse`, `InvalidCredentials`, `AccountNotActive`, `InvalidRefreshToken`, `UserNotFound`).

#### 3. `src/Modules/Identity/Identity.Application`
- `Abstractions/Ports/IUserRepository.cs`: Secondary outbound port defining user and refresh token data access.
- `Abstractions/Ports/IPasswordHasherPort.cs`: Secondary outbound port defining cryptographic hashing and verification contracts.
- `Abstractions/Ports/IJwtProviderPort.cs`: Secondary outbound port defining token pair generation contracts.
- `DTOs/AuthResponseDtos.cs`: Records for `RegisterUserResponse`, `AuthTokenResponse`, `UserContextResponse`, `TokenPairResult`, and `RevokeTokenResponse`.
- `Features/Register/`: `RegisterUserCommand`, `RegisterUserCommandHandler`, `RegisterUserValidator`.
- `Features/Login/`: `LoginCommand`, `LoginCommandHandler`, `LoginValidator`.
- `Features/Refresh/`: `RefreshTokenCommand`, `RefreshTokenCommandHandler`, `RefreshTokenValidator`.
- `Features/Revoke/`: `RevokeTokenCommand`, `RevokeTokenCommandHandler`, `RevokeTokenValidator`.
- `DependencyInjection.cs`: Discovers and registers all MediatR handlers and FluentValidation validators for the Identity module.

#### 4. `src/Modules/Identity/Identity.Infrastructure`
- `Persistence/IdentityDbContext.cs`: EF Core context configuring default schema `"identity"` and entity mappings.
- `Persistence/Configurations/UserConfiguration.cs`: Maps `identity.Users`, column constraints, and unique index `UX_Users_Email`.
- `Persistence/Configurations/RefreshTokenConfiguration.cs`: Maps `identity.RefreshTokens`, unique index `UX_RefreshTokens_Token`, index `IX_RefreshTokens_UserId`, and cascade delete.
- `Persistence/Repositories/UserRepository.cs`: Implements `IUserRepository` via EF Core queries.
- `Security/JwtOptions.cs`: Strongly typed options bound to configuration section `"JwtOptions"`.
- `Security/JwtProviderAdapter.cs`: Generates HMAC-SHA256 JWT access tokens with embedded roles/permissions and cryptographically random 64-byte refresh tokens.
- `Security/BcryptPasswordHasherAdapter.cs`: BCrypt hashing implementation using enhanced work-factor hashing.
- `Integration/UserIdentityAdapter.cs`: Implements cross-module `IUserIdentityPort` (status updates, user verification, and token cascade revocation).
- `DependencyInjection.cs`: Configures DbContext with isolated migration history `__EFMigrationsHistory` in schema `identity` and registers adapters.

#### 5. `src/Modules/AccessControl/AccessControl.Domain`
- `Entities/Role.cs`: Domain entity storing name, normalized uppercase name, description, system flag, and role-permission relations.
- `Entities/Permission.cs`: Domain entity holding unique code (e.g. `roles:write`), display name, module group, and description.
- `Entities/RolePermission.cs`: Composite entity mapping role ID to permission ID with grant timestamp.
- `Entities/UserRole.cs`: Composite entity mapping user UUID to role ID with assignment timestamp.
- `Errors/RbacErrors.cs`: Domain error catalog (`RoleAlreadyExists`, `RoleNotFound`, `SystemRoleModificationForbidden`, `InvalidPermissionCodes`, `RoleAssignmentConflict`).

#### 6. `src/Modules/AccessControl/AccessControl.Application`
- `Abstractions/Ports/IRbacRepository.cs`: Secondary outbound port defining RBAC queries and persistence.
- `DTOs/RbacDtos.cs`: Records for `CreateRoleResponse`, `RoleDto`, `PermissionDto`, `UpdateRolePermissionsResponse`, `AssignUserRoleResponse`, and `UpdateUserStatusResponse`.
- `Features/CreateRole/`: `CreateRoleCommand`, `CreateRoleCommandHandler`, `CreateRoleValidator`.
- `Features/GetRoles/`: `GetRolesQuery`, `GetRolesQueryHandler`.
- `Features/GetPermissions/`: `GetPermissionsQuery`, `GetPermissionsQueryHandler`.
- `Features/UpdateRolePermissions/`: `UpdateRolePermissionsCommand`, `UpdateRolePermissionsCommandHandler` (with SuperAdmin system protection), `UpdateRolePermissionsValidator`.
- `Features/AssignUserRole/`: `AssignUserRoleCommand`, `AssignUserRoleCommandHandler`, `AssignUserRoleValidator`.
- `Features/UpdateUserStatus/`: `UpdateUserStatusCommand`, `UpdateUserStatusCommandHandler`, `UpdateUserStatusValidator`.
- `DependencyInjection.cs`: Discovers and registers all MediatR handlers and FluentValidation validators for the AccessControl module.

#### 7. `src/Modules/AccessControl/AccessControl.Infrastructure`
- `Persistence/AccessControlDbContext.cs`: EF Core context configuring default schema `"rbac"` and entity sets.
- `Persistence/Configurations/`: Entity configurations for `Roles`, `Permissions`, `RolePermissions`, and `UserRoles`.
- `Persistence/Repositories/RbacRepository.cs`: Implements `IRbacRepository` querying relational tables.
- `Integration/RbacAccessAdapter.cs`: Implements cross-module `IRbacAccessPort` fetching user access profiles and assigning default roles.
- `DependencyInjection.cs`: Configures DbContext with isolated migration history `__EFMigrationsHistory` in schema `rbac` and registers repository/adapter.

#### 8. `src/Host/AuthService.API`
- `Authorization/HasPermissionAttribute.cs`: Custom authorization attribute setting policy to permission code.
- `Authorization/PermissionRequirement.cs`: Authorization requirement holding permission code string.
- `Authorization/PermissionAuthorizationHandler.cs`: Evaluates JWT claims for `"permissions"` matching requirement.
- `Authorization/PermissionPolicyProvider.cs`: Dynamically generates authorization policies on-demand.
- `Contracts/`: HTTP request models (`RegisterRequest`, `LoginRequest`, `RefreshTokenRequest`, `RevokeTokenRequest`, `CreateRoleRequest`, etc.).
- `Controllers/AuthController.cs`: REST endpoints for `/api/v1/auth` (`register`, `login`, `refresh`, `revoke`, `me`).
- `Controllers/RbacController.cs`: REST endpoints for `/api/v1/rbac` (`roles`, `permissions`, role permissions, user roles, user status).
- `Extensions/ResultExtensions.cs`: Converts domain/application `Result` failure states into RFC 7807 `ProblemDetails` `ObjectResult`.
- `Extensions/SwaggerExtensions.cs`: Configures OpenAPI 3.0 document and Bearer HTTP security definition.
- `Middleware/GlobalExceptionHandler.cs`: Catches unhandled exceptions, logs them, and returns RFC 7807 500 JSON.
- `Seeding/DatabaseSeeder.cs`: Automates database migrations execution and seeds permissions, roles, and default users upon startup.
- `Program.cs`: Composition root configuring services, authentication, authorization, health checks, Swagger, and middleware pipeline.

---
## 4. Step-by-Step SDLC Execution Journey (How We Built It)

We followed a disciplined, 8-phase software development lifecycle to guarantee incremental correctness, zero circular dependencies, and complete testability at every stage:

```
┌───────────────────────────────────────────────────────────────────────────────────────────┐
│                               8-PHASE IMPLEMENTATION LIFECYCLE                            │
│                                                                                           │
│   Phase 1: Solution Scaffolding & Reference Matrix Configuration                          │
│     ├── Created AuthMicroservice.sln with 8 strictly isolated .NET 8 projects             │
│     └── Enforced Clean Architecture references; zero NuGet packages in Domain projects   │
│                                           │                                               │
│   Phase 2: SharedKernel Implementation    ▼                                               │
│     ├── Base Entity, Result<T> pattern, Error record, and CQRS interfaces                │
│     └── MediatR ValidationBehavior and cross-module ports (IRbacAccessPort, IUserIdentity)│
│                                           │                                               │
│   Phase 3: Module 1 — Identity.Domain & Identity.Application                             │
│     ├── User & RefreshToken entities, UserStatus enum, IdentityErrors catalog             │
│     └── CQRS slices: RegisterUser, Login, RefreshToken, RevokeToken + Validators         │
│                                           │                                               │
│   Phase 4: Module 1 — Identity.Infrastructure                                            │
│     ├── IdentityDbContext (schema: [identity]), UserRepository, BCrypt Hasher Adapter     │
│     └── JwtProviderAdapter (HMAC-SHA256, 64-byte tokens) & UserIdentityAdapter port impl  │
│                                           │                                               │
│   Phase 5: Module 2 — AccessControl.Domain & AccessControl.Application                   │
│     ├── Role, Permission, RolePermission, UserRole entities, RbacErrors catalog          │
│     └── CQRS slices: CreateRole, GetRoles, GetPermissions, UpdateRolePermissions, etc.    │
│                                           │                                               │
│   Phase 6: Module 2 — AccessControl.Infrastructure                                       │
│     ├── AccessControlDbContext (schema: [rbac]), RbacRepository, composite key configs   │
│     └── RbacAccessAdapter (IRbacAccessPort implementation for cross-module profile queries)│
│                                           │                                               │
│   Phase 7: Host API — AuthService.API Composition Root                                    │
│     ├── Dynamic RBAC Authorization: [HasPermission], PolicyProvider & AuthorizationHandler│
│     ├── RFC 7807 ResultExtensions, GlobalExceptionHandler, REST Controllers, Swagger     │
│     └── JWT Bearer setup with MapInboundClaims = false                                    │
│                                           │                                               │
│   Phase 8: Migrations, Automated Seeder & End-to-End Readiness                           │
│     ├── InitialIdentitySchema & InitialRbacSchema migrations generated                    │
│     ├── DatabaseSeeder: auto-migrates and seeds 6 permissions, 3 roles, and 2 users       │
│     └── Health checks mapped to /health; 100% clean rebuild (0 warnings, 0 errors)        │
└───────────────────────────────────────────────────────────────────────────────────────────┘
```

### Detailed Phase Milestones:
1. **Phase 1 (Scaffolding):**
   - Configured solution `AuthMicroservice.sln` with 8 projects.
   - Pinned explicit NuGet versions: `Microsoft.EntityFrameworkCore.SqlServer 8.0.10`, `MediatR 12.4.1`, `FluentValidation 11.10.0`, `BCrypt.Net-Next 4.0.3`, `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.10`, `Swashbuckle.AspNetCore 6.8.1`.
2. **Phase 2 (SharedKernel):**
   - Established the functional `Result<T>` and `Error` foundation to eliminate exception-based flow control.
   - Introduced `ValidationBehavior<TRequest, TResponse>` into the MediatR pipeline.
   - Defined `IRbacAccessPort` and `IUserIdentityPort` contracts to decouple modules.
3. **Phase 3 (Identity Application & Domain):**
   - Built rich domain models with state transition methods (`Activate()`, `Suspend()`, `Deactivate()`, `Revoke()`).
   - Implemented use cases: User registration, password verification, token exchange, and revocation.
4. **Phase 4 (Identity Infrastructure):**
   - Mapped `identity.Users` and `identity.RefreshTokens` in `IdentityDbContext`.
   - Built `JwtProviderAdapter` generating cryptographically random refresh tokens and standards-compliant JWTs.
   - Implemented `UserIdentityAdapter` providing user lookup and active token revocation for AccessControl.
5. **Phase 5 (AccessControl Application & Domain):**
   - Implemented role-based authorization primitives and permission matrix.
   - Enforced business protection for `SuperAdmin`: preventing administrative stripping of core permissions.
6. **Phase 6 (AccessControl Infrastructure):**
   - Mapped `rbac.Roles`, `rbac.Permissions`, `rbac.RolePermissions`, and `rbac.UserRoles` with cascade deletes.
   - Implemented `RbacAccessAdapter` translating identity user IDs into distinct roles and permission codes.
7. **Phase 7 (Host API & Dynamic Authorization):**
   - Implemented `PermissionPolicyProvider` and `PermissionAuthorizationHandler` resolving permissions from JWT claims without database queries.
   - Built `AuthController` and `RbacController` returning RFC 7807 `ProblemDetails`.
   - Configured OpenAPI Bearer authentication in SwaggerGen.
8. **Phase 8 (Database Migrations, Seeder & Verification):**
   - Created isolated EF Core migrations for both contexts.
   - Added `MigrationsHistoryTable("__EFMigrationsHistory", "identity")` and `"rbac"` preventing metadata collision.
   - Developed `DatabaseSeeder` to automatically apply migrations and seed initial matrix data upon startup.

---
## 5. End-to-End Request Execution Flows (Code-Level Trace)

### Flow 1: User Registration (`POST /api/v1/auth/register`)
This trace demonstrates how user creation in `Identity` triggers a cross-boundary assignment in `AccessControl` through Hexagonal Ports without project references.

```
[Client]
   │
   │ 1. POST /api/v1/auth/register {"firstName":"Ahmed","lastName":"Hassan","email":"a@example.com","password":"..."}
   ▼
[AuthController] (API Driving Adapter)
   │ 2. Deserializes RegisterRequest -> instantiates RegisterUserCommand
   │ 3. Dispatches via MediatR: await _sender.Send(command, ct)
   ▼
[ValidationBehavior<RegisterUserCommand, Result<RegisterUserResponse>>] (Pipeline Middleware)
   │ 4. Executes RegisterUserValidator
   │ 5. Validates: Email format, Names <= 100 chars, Password complexity (>=8 chars, [A-Z], [a-z], \d, special char)
   │    (If invalid: immediately short-circuits returning Result.ValidationFailure with errors dictionary)
   ▼
[RegisterUserCommandHandler] (Application Core)
   │ 6. Normalizes email: request.Email.Trim().ToLowerInvariant()
   │ 7. Invokes Outbound Port: await _userRepository.ExistsByEmailAsync(normalizedEmail, ct)
   │ 8. Invokes Outbound Port: string passwordHash = _passwordHasher.HashPassword(request.Password)
   │    └── BcryptPasswordHasherAdapter generates salted hash with work-factor 12
   │ 9. Creates Domain Entity: User.Create(normalizedEmail, passwordHash, firstName, lastName)
   │10. Persists: await _userRepository.AddUserAsync(user, ct); await _userRepository.SaveChangesAsync(ct);
   │    └── IdentityDbContext executes: INSERT INTO [identity].[Users] ...
   │11. CROSS-MODULE HEXAGONAL CALL:
   │    await _rbacAccessPort.AssignDefaultRoleAsync(user.Id, "StandardUser", ct)
   │    └── RbacAccessAdapter (in AccessControl.Infrastructure) queries [rbac].[Roles] for normalized "STANDARDUSER"
   │    └── Inserts UserRole.Create(userId, roleId) into [rbac].[UserRoles]
   │12. Constructs RegisterUserResponse(user.Id, user.Email, "StandardUser", user.CreatedAtUtc)
   │13. Returns Result.Success(response)
   ▼
[AuthController]
   │14. Checks result.IsFailure (false)
   │15. Returns HTTP 201 Created with JSON payload
```

---

### Flow 2: User Login & JWT Issuance (`POST /api/v1/auth/login`)
This trace shows how authentication verifies credentials, fetches permissions across modules, and issues stateless JWTs alongside stateful refresh tokens.

```
[Client]
   │
   │ 1. POST /api/v1/auth/login {"email":"admin@authservice.local","password":"Admin@12345!"}
   ▼
[AuthController]
   │ 2. Dispatches LoginCommand to MediatR
   ▼
[LoginValidator] -> [LoginCommandHandler]
   │ 3. Queries Outbound Port: var user = await _userRepository.GetByEmailAsync(normalizedEmail, ct)
   │    (If user is null -> returns 401 Unauthorized IdentityErrors.InvalidCredentials)
   │ 4. Verifies User Status:
   │    if (user.Status != UserStatus.Active)
   │        return Result.Failure(IdentityErrors.AccountNotActive); // Maps to 403 Forbidden
   │ 5. Verifies Password:
   │    if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
   │        return Result.Failure(IdentityErrors.InvalidCredentials); // 401 Unauthorized
   │ 6. CROSS-MODULE RBAC QUERY:
   │    var accessProfile = await _rbacAccessPort.GetUserAccessProfileAsync(user.Id, ct);
   │    └── RbacAccessAdapter joins [rbac].[UserRoles] -> [rbac].[Roles] -> [rbac].[RolePermissions] -> [rbac].[Permissions]
   │    └── Returns UserAccessProfileDto(["SuperAdmin"], ["auth:profile:read", "users:read", ...])
   │ 7. Invokes Outbound Port: _jwtProvider.GenerateTokenPair(user, accessProfile.Roles, accessProfile.Permissions)
   │    └── JwtProviderAdapter builds claims:
   │        - sub: user.Id.ToString()
   │        - email: user.Email
   │        - name: user.FullName
   │        - jti: Guid.NewGuid().ToString("D")
   │        - roles: ["SuperAdmin"]
   │        - permissions: ["auth:profile:read", "users:read", ...]
   │    └── Signs JWT with HMAC-SHA256 (expires in 15 minutes)
   │    └── Generates 64 cryptographically random bytes encoded in Base64 (expires in 7 days)
   │ 8. Creates Domain Entity:
   │    var refreshToken = RefreshToken.Create(user.Id, tokenPair.RefreshToken, tokenPair.JwtId, tokenPair.ExpiresAtUtc);
   │ 9. Persists:
   │    await _userRepository.AddRefreshTokenAsync(refreshToken, ct);
   │    await _userRepository.SaveChangesAsync(ct);
   │    └── IdentityDbContext executes: INSERT INTO [identity].[RefreshTokens] ...
   │10. Returns Result.Success(AuthTokenResponse)
   ▼
[AuthController]
   │11. Returns HTTP 200 OK with accessToken, refreshToken, expiration dates, and user context
```

---

### Flow 3: Accessing a Protected RBAC Endpoint (`POST /api/v1/rbac/roles`)
This trace demonstrates stateless authorization without database roundtrips on every HTTP request.

```
[Client]
   │
   │ 1. POST /api/v1/rbac/roles {"name":"Auditor","description":"..."}
   │    Header: Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ▼
[ASP.NET Core Authentication Middleware (JwtBearer)]
   │ 2. Intercepts request; extracts Bearer token
   │ 3. Validates Issuer, Audience, SigningKey (HMAC-SHA256), and Expiration (ClockSkew = 0)
   │ 4. Evaluates: options.MapInboundClaims = false
   │    └── Preserves exact claim names ("sub", "roles", "permissions") instead of SOAP/WS-Security URIs
   │ 5. Creates ClaimsPrincipal and assigns to HttpContext.User
   ▼
[ASP.NET Core Authorization Middleware]
   │ 6. Endpoint is decorated with: [HasPermission("roles:write")]
   │ 7. PermissionPolicyProvider.GetPolicyAsync("roles:write") is invoked:
   │    └── Dynamically constructs AuthorizationPolicy:
   │        - RequireAuthenticatedUser()
   │        - AddRequirements(new PermissionRequirement("roles:write"))
   │ 8. PermissionAuthorizationHandler.HandleRequirementAsync is invoked:
   │    └── Evaluates: context.User.Claims.Any(c => c.Type == "permissions" && c.Value == "roles:write")
   │    └── Matches claim -> calls context.Succeed(requirement)
   │    (If user lacked claim: middleware immediately returns 403 Forbidden RFC 7807 ProblemDetails)
   ▼
[RbacController.CreateRole]
   │ 9. Passes CreateRoleCommand to MediatR
   │10. CreateRoleValidator confirms Name is not empty and <= 100 chars
   │11. CreateRoleCommandHandler executes:
   │    - Checks role existence: await _rbacRepository.RoleExistsByNameAsync(request.Name, ct)
   │    - Creates Domain Entity: Role.Create(request.Name, request.Description, isSystemRole: false)
   │    - Persists: await _rbacRepository.AddRoleAsync(role, ct); await _rbacRepository.SaveChangesAsync(ct);
   │12. Returns HTTP 201 Created with CreateRoleResponse
```

---

### Flow 4: Refresh Token Rotation & Replay Protection (`POST /api/v1/auth/refresh`)
This trace demonstrates how stale or replayed refresh tokens are rejected, and valid tokens are single-use and rotated.

```
[Client]
   │
   │ 1. POST /api/v1/auth/refresh {"refreshToken":"dGhpcyBpcyBhIHRlc3QgcmVmcmVzaCB0b2tlbg=="}
   ▼
[AuthController] -> [RefreshTokenCommandHandler]
   │ 2. Queries: var existingToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken, ct)
   │ 3. REPLAY / REVOCATION CHECK:
   │    if (existingToken is null || !existingToken.IsActive)
   │        return Result.Failure(IdentityErrors.InvalidRefreshToken); // Returns 401 Unauthorized
   │    (Note: IsActive is computed as: !IsRevoked && !IsExpired)
   │ 4. Fetches User: var user = await _userRepository.GetByIdAsync(existingToken.UserId, ct)
   │ 5. Verifies User Status:
   │    if (user.Status != UserStatus.Active)
   │        return Result.Failure(IdentityErrors.AccountNotActive); // 403 Forbidden
   │ 6. Queries current roles & permissions: await _rbacAccessPort.GetUserAccessProfileAsync(user.Id, ct)
   │ 7. Generates New Token Pair: _jwtProvider.GenerateTokenPair(...)
   │ 8. ROTATION & AUDIT TRAIL:
   │    existingToken.Revoke("Rotated", newRefreshTokenString);
   │    └── Sets RevokedAtUtc = DateTime.UtcNow
   │    └── Sets ReasonRevoked = "Rotated"
   │    └── Sets ReplacedByToken = newRefreshTokenString
   │ 9. Inserts New Token:
   │    var newRefreshToken = RefreshToken.Create(user.Id, newRefreshTokenString, newJwtId, newExpiresAtUtc);
   │    await _userRepository.AddRefreshTokenAsync(newRefreshToken, ct);
   │    await _userRepository.SaveChangesAsync(ct);
   │10. Returns Result.Success(AuthTokenResponse)
   ▼
[AuthController]
   │11. Returns HTTP 200 OK with new access token and rotated refresh token
```

---

### Flow 5: Suspending a User (`PATCH /api/v1/rbac/users/{userId}/status`)
This trace demonstrates how an administrative action in `AccessControl` triggers immediate session revocation across the `Identity` boundary.

```
[Client (Admin / Manager)]
   │
   │ 1. PATCH /api/v1/rbac/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/status {"status":"Suspended"}
   │    Header: Authorization: Bearer <Admin_JWT>
   ▼
[RbacController] (Requires [HasPermission("users:manage")])
   │ 2. Dispatches UpdateUserStatusCommand(userId, "Suspended") to MediatR
   ▼
[UpdateUserStatusCommandHandler] (AccessControl Application)
   │ 3. CROSS-MODULE OUTBOUND PORT CALL:
   │    var result = await _userIdentityPort.UpdateUserStatusAndRevokeTokensAsync(userId, "Suspended", ct);
   ▼
[UserIdentityAdapter] (Identity Infrastructure Adapter)
   │ 4. Fetches target user: var user = await _userRepository.GetByIdAsync(userId, ct);
   │    (If not found -> returns 404 UserNotFound)
   │ 5. Parses enum: UserStatus.Suspended
   │ 6. Invokes Domain Method: user.Suspend();
   │    └── Sets user.Status = UserStatus.Suspended
   │    └── Sets user.UpdatedAtUtc = DateTime.UtcNow
   │ 7. CASCADE REVOCATION:
   │    int revokedCount = await _userRepository.RevokeAllActiveUserTokensAsync(userId, "AdminSuspended", ct);
   │    └── Executes UPDATE [identity].[RefreshTokens]
   │        SET RevokedAtUtc = GETUTCDATE(), ReasonRevoked = 'AdminSuspended'
   │        WHERE UserId = @userId AND RevokedAtUtc IS NULL AND ExpiresAtUtc > GETUTCDATE()
   │ 8. Persists changes: await _userRepository.SaveChangesAsync(ct);
   │ 9. Returns Result.Success(revokedCount);
   ▼
[UpdateUserStatusCommandHandler]
   │10. Constructs UpdateUserStatusResponse(userId, "Suspended", revokedActiveTokensCount)
   │11. Returns Result.Success(...)
   ▼
[RbacController]
   │12. Returns HTTP 200 OK
   │    (Any subsequent request using the suspended user's refresh token will immediately fail with 403 AccountNotActive)
```

---
## 6. Live Swagger Demo Script (Step-by-Step Presentation Guide)

Follow this chronological 10-step script during your architecture review or technical interview to demonstrate end-to-end functionality live in Swagger UI:

### Step 1: Boot the Application & Highlight Self-Healing Startup
1. Open terminal at repository root and run:
   ```bash
   dotnet run --project src/Host/AuthService.API
   ```
2. **Talking Point for Interviewer:**  
   *"Notice our `DatabaseSeeder` executing automatically in the console output. It calls `MigrateAsync()` on both isolated DbContexts (`IdentityDbContext` and `AccessControlDbContext`) and idempotently seeds our 6 system permissions, 3 default roles, and 2 default user accounts without throwing errors or duplicating data."*

---

### Step 2: Open Swagger UI & Review Security Schemes
1. Open browser at `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`).
2. Point out the clear grouping into `/api/v1/auth` and `/api/v1/rbac` tags.
3. Point out the top-right green **Authorize** button. Explain that `SwaggerExtensions` configured OpenAPI 3.0 Bearer security definitions so tokens can be tested directly in browser.

---

### Step 3: Authenticate as Default SuperAdmin
1. Expand `POST /api/v1/auth/login`.
2. Click **Try it out** and execute with:
   ```json
   {
     "email": "admin@authservice.local",
     "password": "Admin@12345!"
   }
   ```
3. **Response:** Status `200 OK`.
4. Copy the `accessToken` string value from the JSON response.
5. **Talking Point for Interviewer:**  
   *"Notice that the login response returns a stateless JWT access token (15-min life) and a stateful Base64-encoded 64-byte refresh token (7-day life), along with the user's role (`SuperAdmin`) and all 6 assigned permissions."*

---

### Step 4: Authorize Swagger UI Session
1. Click the green **Authorize** button at the top of Swagger UI.
2. In the Value box, enter:
   ```
   Bearer <paste_your_copied_access_token>
   ```
3. Click **Authorize**, then click **Close**. All protected endpoints now automatically transmit this token in their `Authorization` header.

---

### Step 5: Verify Stateless Identity & Claims via `/me`
1. Expand `GET /api/v1/auth/me`.
2. Click **Try it out** and click **Execute**.
3. **Response:** Status `200 OK`:
   ```json
   {
     "userId": "...",
     "email": "admin@authservice.local",
     "roles": ["SuperAdmin"],
     "permissions": [
       "auth:profile:read",
       "users:read",
       "users:manage",
       "roles:read",
       "roles:write",
       "roles:manage"
     ]
   }
   ```
4. **Talking Point for Interviewer:**  
   *"Because we configured `MapInboundClaims = false`, our controller directly extracted the clean claim names (`sub`, `roles`, `permissions`) from `HttpContext.User` without querying SQL Server."*

---

### Step 6: Query Seeded RBAC Catalog
1. Expand `GET /api/v1/rbac/permissions` -> Execute.
   - Shows the 6 seeded permissions across `Identity` and `AccessControl` groups.
2. Expand `GET /api/v1/rbac/roles` -> Execute.
   - Shows `SuperAdmin` (all 6 permissions), `Manager` (3 permissions), and `StandardUser` (1 permission).

---

### Step 7: Create a Custom Role & Bind Permissions
1. Expand `POST /api/v1/rbac/roles`.
2. Enter:
   ```json
   {
     "name": "Auditor",
     "description": "Compliance read-only auditor role."
   }
   ```
3. Execute -> Returns `201 Created` with `roleId` (e.g. `7c9e6679-7425-40de-944b-e07fc1f90ae7`).
4. Copy `roleId`.
5. Expand `PUT /api/v1/rbac/roles/{roleId}/permissions`.
6. Paste `roleId` into path parameter and enter body:
   ```json
   {
     "permissionCodes": [
       "auth:profile:read",
       "users:read",
       "roles:read"
     ]
   }
   ```
7. Execute -> Returns `200 OK` confirming permissions bound to the custom role.

---

### Step 8: Register a New User & Inspect Default Assignment
1. Expand `POST /api/v1/auth/register`.
2. Enter:
   ```json
   {
     "firstName": "Tariq",
     "lastName": "Mansour",
     "email": "tariq.mansour@example.com",
     "password": "Secure@Pass123!"
   }
   ```
3. Execute -> Returns `201 Created`:
   ```json
   {
     "userId": "a1b2c3d4-...",
     "email": "tariq.mansour@example.com",
     "assignedDefaultRole": "StandardUser",
     "createdAtUtc": "..."
   }
   ```
4. Copy Tariq's `userId`.
5. **Talking Point for Interviewer:**  
   *"Notice that Tariq was automatically assigned the default `StandardUser` role across bounded contexts via `IRbacAccessPort`."*

---

### Step 9: Demonstrate Real RBAC Enforcement (403 Forbidden)
1. In Swagger UI, click **Authorize** -> **Logout** to clear the Admin token.
2. Authenticate as Tariq via `POST /api/v1/auth/login` (`tariq.mansour@example.com` / `Secure@Pass123!`).
3. Copy Tariq's token, click **Authorize**, paste `Bearer <tariq_token>`, and click **Authorize**.
4. Attempt to create a role via `POST /api/v1/rbac/roles` (`{"name":"HackerRole"}`).
5. Execute -> **Response:** `403 Forbidden` RFC 7807 `ProblemDetails`:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
     "title": "Forbidden",
     "status": 403,
     "detail": "...",
     "instance": "/api/v1/rbac/roles"
   }
   ```
6. **Talking Point for Interviewer:**  
   *"Tariq only possesses `auth:profile:read`. The endpoint requires `[HasPermission("roles:write")]`. The request was stopped dead at the ASP.NET Core authorization pipeline without executing any controller or database code."*

---

### Step 10: Elevate Role, Test Suspension & Validation Errors
1. Re-authenticate as `SuperAdmin` (`admin@authservice.local`).
2. Assign the newly created `Auditor` role to Tariq:
   - Call `POST /api/v1/rbac/users/{userId}/roles` with Tariq's `userId` and `Auditor`'s `roleId`. Returns `200 OK`.
3. Suspend Tariq's account:
   - Call `PATCH /api/v1/rbac/users/{userId}/status` with `{"status": "Suspended"}`.
   - Returns `200 OK` with `revokedActiveTokensCount: 1`.
4. Demonstrate RFC 7807 Validation Errors:
   - Call `POST /api/v1/auth/register` with an invalid password (e.g. `"123"`).
   - Returns `400 Bad Request` with `errorCode = "Identity.ValidationError"` and detailed field error list (`MinimumLength`, `Uppercase`, `SpecialChar`).

---
## 7. Top Technical Interview Q&A (Defense Cheat Sheet)

Review these 10 architectural questions and model answers prior to your presentation:

---

### Q1: "How do you combine Hexagonal Architecture and Clean Architecture in this codebase?"
**Model Answer:**  
*"Clean Architecture dictates the concentric layered dependency rule: Domain has zero dependencies, Application depends only on Domain, and Infrastructure/API depends on Application. Hexagonal Architecture focuses on the boundary interactions through Ports (Interfaces) and Adapters (Implementations).*  
*In our codebase, the Application layer defines Inbound Ports (`ICommand<T>`, `IQuery<T>` handled by MediatR) and Outbound Ports (`IUserRepository`, `IPasswordHasherPort`, `IJwtProviderPort`, `IRbacRepository`). The API controllers act as Primary Driving Adapters feeding data into Inbound Ports. The Infrastructure classes (`UserRepository`, `BcryptPasswordHasherAdapter`, `JwtProviderAdapter`) act as Secondary Driven Adapters implementing Outbound Ports. This ensures that domain and business rules remain 100% agnostic of external technologies like EF Core, BCrypt, or HTTP frameworks."*

---

### Q2: "How do the `Identity` and `AccessControl` modules communicate without creating tight coupling or circular references?"
**Model Answer:**  
*"They communicate strictly through the Inversion of Control and Hexagonal Ports defined in `SharedKernel.Contracts`. Neither module references the other's `.csproj` files or database tables.*  
*For instance, when a user registers, `RegisterUserCommandHandler` needs to assign the default role. Instead of referencing `AccessControl.Domain.Role`, it calls `IRbacAccessPort.AssignDefaultRoleAsync(userId, "StandardUser")`. `RbacAccessAdapter` in `AccessControl.Infrastructure` implements that port. Conversely, when `AccessControl` suspends a user, it calls `IUserIdentityPort.UpdateUserStatusAndRevokeTokensAsync(userId, "Suspended")`, which is implemented by `UserIdentityAdapter` in `Identity.Infrastructure`. At the database level, `rbac.UserRoles` stores `UserId` as an unconstrained UUID, meaning there are no physical cross-schema foreign keys."*

---

### Q3: "Why didn't you use Microsoft's built-in ASP.NET Core Identity (`IdentityDbContext`, `IdentityUser`)?"
**Model Answer:**  
*"ASP.NET Core Identity is heavily opinionated and violates Clean Architecture principles: it couples domain entities to framework classes (`IdentityUser<string>`), forces a string-based primary key or complex generic overrides, generates 8 rigid database tables (such as UserLogins and UserClaims) that we don't need, and mixes UI, identity, and token logic into one bloated abstraction.*  
*By designing custom Domain Entities (`User`, `RefreshToken`, `Role`, `Permission`), we achieved 100% domain encapsulation: all properties have private setters, constructors are private, invariants are guarded via static factory methods, and we have complete control over SQL Server schemas, indexing strategies, and cryptographic token rotation."*

---

### Q4: "How does your dynamic permission authorization work without querying SQL Server on every HTTP request?"
**Model Answer:**  
*"We implement stateless authorization by embedding granular permission codes directly inside the JWT access token payload under the `"permissions"` claim array during login or token refresh.*  
*On subsequent HTTP requests, when an endpoint is decorated with `[HasPermission("roles:write")]`, our custom `PermissionPolicyProvider` dynamically constructs an `AuthorizationPolicy` with a `PermissionRequirement("roles:write")`. Then, `PermissionAuthorizationHandler` simply inspects `context.User.Claims` in-memory. If a claim with `Type == "permissions"` and `Value == "roles:write"` is present, it succeeds. Because cryptographic token signature verification happens in the ASP.NET Core JWT middleware, the authorization decision is instantaneous and requires zero SQL Server database queries."*

---

### Q5: "What is `MapInboundClaims = false` in `Program.cs`, and what happens if you omit it?"
**Model Answer:**  
*"By default, ASP.NET Core's legacy JWT handler maps standard OpenID Connect / RFC 7519 claim names into long, legacy WS-Security / SOAP XML namespaces. For instance, `"sub"` becomes `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`, and `"roles"` becomes `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`.*  
*Setting `options.MapInboundClaims = false` disables this legacy translation. It preserves exact JSON claim names as emitted by `JwtProviderAdapter` (`"sub"`, `"email"`, `"roles"`, `"permissions"`). This allows our `/me` endpoint and `PermissionAuthorizationHandler` to read `c.Type == "permissions"` cleanly without depending on brittle SOAP namespace strings."*

---

### Q6: "How does your refresh token rotation protect against token theft and replay attacks?"
**Model Answer:**  
*"Refresh tokens are single-use cryptographically random 64-byte tokens. In `RefreshTokenCommandHandler`:*  
*1. When a client presents a refresh token, we verify `existingToken.IsActive` (`!IsRevoked && !IsExpired`).*  
*2. We immediately revoke the presented token by calling `existingToken.Revoke("Rotated", newRefreshToken)`.*  
*3. We issue a new access token and a brand new refresh token with a unique `jti`.*  
*If a malicious actor intercepts a refresh token and attempts to replay it after the legitimate client has rotated it, `existingToken.IsActive` returns false because `RevokedAtUtc` is already populated. The request is immediately rejected with HTTP 401 Unauthorized (`Identity.InvalidRefreshToken`). Furthermore, `ReasonRevoked` and `ReplacedByToken` provide an immutable audit trail for security incident forensics."*

---

### Q7: "How do you handle multi-DbContext migrations when both contexts share the same physical database?"
**Model Answer:**  
*"By default, EF Core creates a single `dbo.__EFMigrationsHistory` table. If multiple DbContexts (`IdentityDbContext` and `AccessControlDbContext`) run migrations against the same database, they overwrite each other's migration records, leading to failed table creation errors.*  
*We solved this by configuring isolated migration history tables in each module's `DependencyInjection.cs`:*  
`- For IdentityDbContext: sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "identity")`  
`- For AccessControlDbContext: sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "rbac")`  
*This guarantees that each module maintains an independent migration ledger in its own schema without conflicting."*

---

### Q8: "Why did you implement the `Result<T>` pattern instead of throwing custom exceptions for validation and business errors?"
**Model Answer:**  
*"In .NET, exceptions are designed for exceptional, unforeseen runtime disasters (e.g. database network loss, disk out of space). Throwing exceptions for predictable business logic (e.g. invalid password, duplicate email, account suspended) has severe architectural downsides:*  
*1. **Performance Cost:** Capturing stack traces across frames imposes significant CPU and memory overhead.*  
*2. **Control Flow Obfuscation:** It creates invisible `goto` jumps through the call stack.*  
*3. **Type Safety:** Method signatures don't indicate what exceptions they might throw.*  
*With `Result` and `Result<TValue>`, business failures are first-class return types (`Result.Failure(IdentityErrors.InvalidCredentials)`). The controller or caller explicitly handles both paths. We only let the pipeline throw for unhandled runtime crashes, which are caught and converted into RFC 7807 500 Internal Server Error payloads by `GlobalExceptionHandler`."*

---

### Q9: "How did you prevent an administrator from accidentally stripping permissions from the `SuperAdmin` role?"
**Model Answer:**  
*"In `UpdateRolePermissionsCommandHandler`, we implemented a domain protection check before modifying role permissions:*  
```csharp
if (role.IsSystemRole && role.NormalizedName == "SUPERADMIN")
{
    var missingCorePermissions = SuperAdminCorePermissions
        .Where(cp => !request.PermissionCodes.Contains(cp, StringComparer.OrdinalIgnoreCase))
        .ToList();

    if (missingCorePermissions.Count > 0)
    {
        return Result.Failure<UpdateRolePermissionsResponse>(RbacErrors.SystemRoleModificationForbidden);
    }
}
```
*If a request attempts to update `SuperAdmin` without retaining all 6 core system permissions, the command immediately aborts with `RbacErrors.SystemRoleModificationForbidden` (HTTP 403 Forbidden). This guarantees that the system can never be locked out of administrative governance."*

---

### Q10: "If this microservice needs to scale to millions of concurrent users, what are the bottlenecks and how would you evolve it?"
**Model Answer:**  
*"Our architecture was deliberately designed for easy horizontal scaling:*  
*1. **Stateless Authorization:** Because access tokens carry permissions in their claims, authorization checks require zero database hits. We can scale the `AuthService.API` host to 100+ container instances behind a load balancer immediately.*  
*2. **Distributed Token Invalidation:** Currently, refresh tokens and user suspensions update SQL Server. At high scale, we would introduce a distributed cache (e.g. Redis Cluster) to store active refresh tokens and a lightweight revocation bloom filter / blacklist, keeping validation latencies sub-millisecond.*  
*3. **Database Read/Write Splitting:** We can configure SQL Server AlwaysOn Availability Groups, routing queries (`GetRolesQuery`, `GetPermissionsQuery`) to read-only replicas while directing writes (`RegisterUserCommand`, `LoginCommand`) to the primary replica.*  
*4. **Event-Driven Decoupling:** The cross-module synchronous calls (`IRbacAccessPort`, `IUserIdentityPort`) can seamlessly be transitioned into asynchronous event publishing (e.g. via RabbitMQ or Azure Service Bus) using an Outbox Pattern if the two modules are eventually deployed as separate standalone microservices."*

---