# Enterprise Authentication, Users & Role-Based Access Control (RBAC) Microservice

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Modular%20Clean%20%2F%20Hexagonal-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![EF Core 8](https://img.shields.io/badge/EF%20Core-8.0-purple)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Production-grade, enterprise-scale Modular Clean Architecture Microservice unifying **Identity & Authentication**, **User Profiles**, and **Dynamic RBAC Authorization** built with **.NET 8**, **Hexagonal Ports & Adapters**, and **CQRS (MediatR)**.

---

## 📑 Table of Contents
- [🏛️ Unified Architecture Overview](#️-unified-architecture-overview)
- [🧩 Module Boundaries & Schema Isolation](#-module-boundaries--schema-isolation)
- [📂 Solution Directory Structure](#-solution-directory-structure)
- [🚀 Quick Start Guide](#-quick-start-guide)
- [🔑 Pre-Seeded Accounts & Credentials](#-pre-seeded-accounts--credentials)
- [🛡️ RBAC Permission Matrix](#️-rbac-permission-matrix)
- [📡 API Endpoints Reference](#-api-endpoints-reference)
  - [1. Authentication & Identity (`/api/v1/auth`)](#1-authentication--identity-apiv1auth)
  - [2. User Profiles (`/api/v1/users`)](#2-user-profiles-apiv1users)
  - [3. Access Control & RBAC (`/api/v1/rbac`)](#3-access-control--rbac-apiv1rbac)
  - [4. Operational Health Checks (`/health`)](#4-operational-health-checks-health)
- [🔒 Security & Production Engineering Highlights](#-security--production-engineering-highlights)
- [🧪 Testing in Swagger UI & Browser](#-testing-in-swagger-ui--browser)

---

## 🏛️ Unified Architecture Overview

The microservice follows strict **Modular Clean Architecture** and **Hexagonal (Ports & Adapters)** principles. The host (`AuthService.API`) bootstraps three autonomous business modules, each composed of isolated `.Domain`, `.Application`, `.Infrastructure`, and `.Presentation` class libraries:

```
                            +-----------------------------------------------+
                            |             Host (AuthService.API)            |
                            |  Middleware, Dual Token Extractor, Rate Limits|
                            +-----------------------+-----------------------+
                                                    |
         +------------------------------------------+------------------------------------------+
         |                                          |                                          |
         v                                          v                                          v
+-------------------------+        +----------------------------------+        +-------------------------+
|     Identity Module     |        |      Access Control (RBAC)       |        |      Users Module       |
| [identity] DbContext    |        |        [rbac] DbContext          |        |    [users] DbContext    |
| - Password & Mobile Auth|        | - Granular Permission Catalog    |        | - User Profile Aggregate|
| - UltraMsg WhatsApp OTP |        | - Roles & Role-Permissions       |        | - Contact Information   |
| - Smtp Password Reset   |        | - User-to-Role Dynamic Mappings  |        | - Profile Pagination    |
| - 5 OAuth2 Providers    |        | - Token Revocation Cascade       |        | - Soft-Delete/Restore   |
| - LoginSession Tracking |        | - Permission Requirement Handler |        | - Cross-Module Sync Port|
+-------------------------+        +----------------------------------+        +-------------------------+
         ^                                          ^                                          ^
         +------------------------------------------+------------------------------------------+
                                                    |
                                    +---------------+---------------+
                                    |    SharedKernel Contracts     |
                                    | IUserProfilePort, IRbacPort,  |
                                    | IUserIdentityPort, Result<T>  |
                                    +-------------------------------+
```

---

## 🧩 Module Boundaries & Schema Isolation

The database is segregated into three completely independent SQL Server schemas with **zero cross-schema foreign keys**:

1. **`identity` Schema (`IdentityDbContext`):**
   - `identity.Users` — Aggregate root with email, BCrypt password hash, country code, phone number, verification flags, and status.
   - `identity.RefreshTokens` — Cryptographic refresh tokens with automatic rotation and revocation.
   - `identity.OtpVerifications` — SHA-256 hashed 6-digit codes with 5-minute expiry, purpose tracking, and attempt limits.
   - `identity.PasswordResetTokens` — SHA-256 hashed reset & invite tokens with 15-minute / 24-hour expiration.
   - `identity.LoginSessions` — Audit logs tracking device names, client IP addresses, user agents, and session lifecycles.
   - `identity.ExternalLogins` — OAuth2 provider bindings (`Google`, `Facebook`, `GitHub`, `LinkedIn`, `Microsoft`).

2. **`rbac` Schema (`AccessControlDbContext`):**
   - `rbac.Roles` — System roles (`SuperAdmin`, `Manager`, `StandardUser`) and custom administrator-defined roles.
   - `rbac.Permissions` — Granular permission catalog codes.
   - `rbac.RolePermissions` — Many-to-many role-to-permission mappings.
   - `rbac.UserRoles` — User-to-role assignments mapped via unconstrained `UserId` GUIDs.

3. **`users` Schema (`UsersDbContext`):**
   - `users.UserProfiles` — Isolated profile aggregate root containing user email, full name, phone number, and soft-delete audit trail.

---

## 📂 Solution Directory Structure

```plaintext
AuthMicroservice/
├── Docs/                                          # Architecture, PRD, and Unification specifications
│   ├── 08-Team-Unification-Master-Plan.md
│   └── 09-Unification-Agent-Prompts.md
├── src/
│   ├── Shared/
│   │   └── SharedKernel/                          # Result pattern, Base Entity, CQRS Primitives, Presentation helpers
│   ├── Modules/
│   │   ├── Identity/
│   │   │   ├── Identity.Domain/                   # User, RefreshToken, OtpVerification, PasswordResetToken, ExternalLogin
│   │   │   ├── Identity.Application/              # CQRS handlers, FluentValidation, Notification & Social ports
│   │   │   ├── Identity.Infrastructure/           # EF Core IdentityDbContext, UltraMsg, SMTP, BCrypt, 5 Social Adapters
│   │   │   └── Identity.Presentation/             # AuthController & IAuthCookieService
│   │   ├── AccessControl/
│   │   │   ├── AccessControl.Domain/              # Role, Permission, UserRole entities & domain logic
│   │   │   ├── AccessControl.Application/         # RBAC CQRS commands/queries & validators
│   │   │   ├── AccessControl.Infrastructure/      # EF Core AccessControlDbContext & Repositories
│   │   │   └── AccessControl.Presentation/        # RbacController
│   │   └── Users/
│   │       ├── Users.Domain/                      # UserProfile entity & domain errors
│   │       ├── Users.Application/                 # Profile CQRS commands/queries & ports
│   │       ├── Users.Infrastructure/              # EF Core UsersDbContext, Repositories & UserProfileAdapter
│   │       └── Users.Presentation/                # UsersController
│   └── Host/
│       └── AuthService.API/                       # Web API Host, Rate Limiting, Dual Token Extractor, Seeder
├── AuthMicroservice.sln                           # Unified Solution (14 projects)
└── README.md
```

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, SQL Express, or Docker SQL Server)

### 1. Configuration
Configure database connection string, JWT signing keys, and external service credentials in [`src/Host/AuthService.API/appsettings.json`](src/Host/AuthService.API/appsettings.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AuthMicroserviceDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtOptions": {
    "SecretKey": "EnterpriseSuperSecretAuthenticationSigningKeyMustBeVeryLong2026!",
    "Issuer": "AuthMicroservice",
    "Audience": "EnterpriseMicroservices",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "UltraMsg": {
    "InstanceId": "",
    "Token": "",
    "BaseUrl": "https://api.ultramsg.com"
  },
  "Smtp": {
    "Host": "",
    "Port": 587,
    "Username": "",
    "Password": "",
    "FromEmail": "noreply@authservice.com",
    "FromName": "Auth Service",
    "EnableSsl": true,
    "FrontendResetUrl": "http://localhost:4200/#/set-password"
  },
  "Authentication": {
    "Google": { "ClientId": "" },
    "Facebook": { "AppId": "", "AppSecret": "" },
    "GitHub": { "ClientId": "", "ClientSecret": "", "RedirectUri": "" },
    "LinkedIn": { "ClientId": "", "ClientSecret": "", "RedirectUri": "" },
    "Microsoft": { "ClientId": "", "ClientSecret": "", "RedirectUri": "", "TenantId": "common" }
  }
}
```

### 2. Build the Entire Solution
```bash
dotnet build AuthMicroservice.sln
```

### 3. Run the Microservice
```bash
dotnet run --project src/Host/AuthService.API
```

> **Automatic Multi-Context Migration & Seeding:** On startup, `DatabaseSeeder` automatically runs EF Core migrations for `[identity]`, `[rbac]`, and `[users]` schemas, populating permissions, system roles, default accounts, and synchronized user profiles.

---

## 🔑 Pre-Seeded Accounts & Credentials

The service seeds two pre-configured, verified accounts out of the box:

| Role | Email | Password | Phone | Assigned Permissions |
| :--- | :--- | :--- | :--- | :--- |
| **SuperAdmin** | `admin@authservice.local` | `Admin@12345!` | `+20 1000000001` (Verified) | `auth:profile:read`, `users:read`, `users:manage`, `roles:read`, `roles:write`, `roles:manage` |
| **StandardUser** | `user@authservice.local` | `User@12345!` | `+20 1000000002` (Verified) | `auth:profile:read` |

---

## 🛡️ RBAC Permission Matrix

| Permission Code | Module Group | Description | SuperAdmin | Manager | StandardUser |
| :--- | :--- | :--- | :---: | :---: | :---: |
| `auth:profile:read` | `Identity` | Read own authenticated profile (`/me`) | ✅ | ✅ | ✅ |
| `users:read` | `AccessControl` | View users, profiles, and role assignments | ✅ | ✅ | ❌ |
| `users:manage` | `AccessControl` | Create/edit profiles, assign roles & modify status | ✅ | ✅ | ❌ |
| `roles:read` | `AccessControl` | View all roles and permission catalog | ✅ | ✅ | ❌ |
| `roles:write` | `AccessControl` | Create new custom roles | ✅ | ❌ | ❌ |
| `roles:manage` | `AccessControl` | Bind/unbind permissions to roles | ✅ | ❌ | ❌ |

---

## 📡 API Endpoints Reference

### 1. Authentication & Identity (`/api/v1/auth`)
| Method | Endpoint | Description | Rate Limit Policy | Auth Required |
| :--- | :--- | :--- | :--- | :---: |
| `POST` | `/api/v1/auth/register` | Register new account & dispatch WhatsApp OTP | `AuthEmail` (3/min) | Anonymous |
| `POST` | `/api/v1/auth/login` | Authenticate with email/password; returns JWT + sets HttpOnly cookies | `AuthLogin` (5/min) | Anonymous |
| `POST` | `/api/v1/auth/login/mobile` | Request 6-digit WhatsApp OTP login code | `AuthLogin` (5/min) | Anonymous |
| `POST` | `/api/v1/auth/verify-otp` | Verify OTP code hash and authenticate session | `AuthSensitive` (5/5min) | Anonymous |
| `POST` | `/api/v1/auth/resend-otp` | Regenerate and re-send active OTP code | `AuthEmail` (3/min) | Anonymous |
| `POST` | `/api/v1/auth/social-login` | OAuth2 social login (`google`, `facebook`, `github`, `linkedin`, `microsoft`) | `AuthLogin` (5/min) | Anonymous |
| `POST` | `/api/v1/auth/forgot-password` | Request password reset email (enumeration-safe) | `AuthEmail` (3/min) | Anonymous |
| `POST` | `/api/v1/auth/reset-password` | Reset password using cryptographically secure token | `AuthSensitive` (5/5min) | Anonymous |
| `POST` | `/api/v1/auth/refresh` | Refresh JWT tokens (reads from JSON body or `refreshToken` cookie) | Standard | Anonymous |
| `POST` | `/api/v1/auth/revoke` | Revoke refresh token and clear cookies | Standard | `[Authorize]` |
| `GET` | `/api/v1/auth/me` | Read current user identity, roles, and claims | Standard | `[HasPermission("auth:profile:read")]` |

### 2. User Profiles (`/api/v1/users`)
| Method | Endpoint | Description | Permission Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/v1/users` | Create user profile with phone and sync to Identity | `users:manage` |
| `GET` | `/api/v1/users` | List paginated user profiles (`page`, `pageSize`, `search`) | `users:read` |
| `GET` | `/api/v1/users/{userId}` | Get user profile by user GUID | `users:read` |
| `GET` | `/api/v1/users/by-phone` | Lookup user profile by country code and phone | `users:read` |
| `PUT` | `/api/v1/users/{userId}` | Update user profile contact details | `users:manage` |
| `DELETE` | `/api/v1/users/{userId}` | Soft-delete user profile and revoke active tokens | `users:manage` |

### 3. Access Control & RBAC (`/api/v1/rbac`)
| Method | Endpoint | Description | Permission Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/v1/rbac/roles` | Create a new custom role | `roles:write` |
| `GET` | `/api/v1/rbac/roles` | List all roles with assigned permissions | `roles:read` |
| `GET` | `/api/v1/rbac/permissions` | List all granular system permissions | `roles:read` |
| `PUT` | `/api/v1/rbac/roles/{roleId}/permissions` | Update permissions assigned to a role | `roles:manage` |
| `POST` | `/api/v1/rbac/users/{userId}/roles` | Assign a role to a user | `users:manage` |
| `PATCH` | `/api/v1/rbac/users/{userId}/status` | Update user status (`Active`/`Suspended`/`Deactivated`) & cascade token revocation | `users:manage` |

### 4. Operational Health Checks (`/health`)
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `GET` | `/health` | Verifies database connectivity and readiness across `[identity]`, `[rbac]`, and `[users]` | Anonymous |

---

## 🔒 Security & Production Engineering Highlights

- **Pre-Hijacking Attack Defense:** When linking a verified social provider to an unverified existing local account, the system automatically invalidates any pre-registered attacker password before verifying the email.
- **SHA-256 Hashing for OTPs & Reset Tokens:** OTP codes and password reset tokens are never stored in plaintext in the database.
- **Strict SameSite HttpOnly Cookies:** JWT tokens are dispatched both in the JSON payload (for mobile/Swagger) and inside `SameSite = Strict; HttpOnly; Secure` cookies.
- **Dual Token Extraction Middleware:** The API automatically extracts JWTs from either the `Authorization: Bearer <token>` header or the `accessToken` HttpOnly cookie.
- **IP Partition Rate Limiting:** Enforces strict fixed-window rate limiting on login, registration, and password reset endpoints returning RFC 7807 `429 Too Many Requests`.
- **Enumeration Attack Prevention:** The `/forgot-password` endpoint returns identical success responses regardless of whether the email is registered.
- **Zero Cross-Schema Foreign Keys:** Strict database logical boundary isolation enabling independent scaling and splitting into separate physical microservices with zero schema redesign.

---

## 🧪 Testing in Swagger UI & Browser

1. Run the application and navigate to `http://localhost:5000/swagger`.
2. Execute `POST /api/v1/auth/login` using the pre-seeded credentials:
   ```json
   {
     "email": "admin@authservice.local",
     "password": "Admin@12345!"
   }
   ```
3. The response will include both the `accessToken` JSON payload and `Set-Cookie` response headers.
4. Copy the `accessToken`, click **Authorize** at the top right, enter `Bearer <your_token>`, and test protected endpoints across `/api/v1/auth/me`, `/api/v1/users`, and `/api/v1/rbac/roles`.