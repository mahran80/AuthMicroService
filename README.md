# Enterprise Authentication & Role-Based Access Control (RBAC) Microservice

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Hexagonal%20%2F%20Clean-blue)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![EF Core 8](https://img.shields.io/badge/EF%20Core-8.0-purple)](https://learn.microsoft.com/ef/core/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

Production-grade, enterprise-scale Authentication and Dynamic RBAC Authorization Microservice built with **.NET 8**, **Clean Architecture**, **Hexagonal Ports & Adapters**, and **CQRS (MediatR)**.

---

## 📑 Table of Contents
- [🏛️ Architecture Overview](#️-architecture-overview)
- [📂 Solution Directory Structure](#-solution-directory-structure)
- [📚 Comprehensive Documentation](#-comprehensive-documentation)
- [🚀 Quick Start Guide](#-quick-start-guide)
- [🔑 Pre-Seeded Accounts & Credentials](#-pre-seeded-accounts--credentials)
- [🛡️ RBAC Permission Matrix](#️-rbac-permission-matrix)
- [📡 API Endpoints Reference](#-api-endpoints-reference)
- [🔒 Security & Production Engineering Highlights](#-security--production-engineering-highlights)
- [🧪 Testing in Swagger UI](#-testing-in-swagger-ui)

---

## 🏛️ Architecture Overview

The microservice follows strict **Clean Architecture** and **Hexagonal (Ports & Adapters)** principles with complete domain isolation:

```
                            +-----------------------------------+
                            |        Primary / Driving          |
                            |   (AuthService.API Controllers)   |
                            +-----------------+-----------------+
                                              |
                                              v
                            +-----------------+-----------------+
                            |     Application Inbound Ports     |
                            |         (MediatR CQRS)            |
                            +-----------------+-----------------+
                                              |
                                              v
                            +-----------------+-----------------+
                            |          Domain Core              |
                            |   (Pure C# - Zero Dep on EF/Web)  |
                            +-----------------+-----------------+
                                              ^
                                              |
                            +-----------------+-----------------+
                            |    Secondary / Driven Adapters    |
                            |   (EF Core 8, BCrypt, JWT Provider)|
                            +-----------------------------------+
```

### Module Boundaries & Schema Isolation
The database is partitioned into two isolated schemas with **zero cross-schema foreign keys**:
1. **`identity` Schema:**
   - `identity.Users` — UUID primary key, unique email index, BCrypt password hash, user status.
   - `identity.RefreshTokens` — Cryptographic refresh tokens with automatic rotation, revocation, and device tracking.
2. **`rbac` Schema:**
   - `rbac.Roles` — System roles and dynamic custom roles.
   - `rbac.Permissions` — Granular permission catalog.
   - `rbac.RolePermissions` — Role-to-permission mapping.
   - `rbac.UserRoles` — User-to-role assignment using unconstrained UUIDs across boundary.

### Cross-Module Communication (Hexagonal Ports)
- Modules interact strictly via Outbound Ports defined in `SharedKernel.Contracts`:
  - `IRbacAccessPort`: Used by Identity to fetch user roles/permissions and assign default roles on registration.
  - `IUserIdentityPort`: Used by AccessControl to verify user existence and cascade revocation of active tokens upon status modification.

---

## 📂 Solution Directory Structure

```plaintext
AuthMicroservice/
├── Docs/                                 # Complete architecture & defense documentation
│   ├── 01-PRD-and-Requirements.md
│   ├── 02-Architecture-and-Hexagonal-Design.md
│   ├── 03-State-and-Sequence-Diagrams.md
│   ├── 04-Database-Schema-and-DDL.md
│   ├── 05-API-Contracts-and-Swagger-Spec.md
│   ├── 06-Master-Implementation-Plan.md
│   ├── 07-Agent-Execution-Prompts.md
│   └── 08-Complete-Project-Walkthrough-and-Defense-Guide.md
├── src/
│   ├── Shared/
│   │   └── SharedKernel/                 # Shared Result pattern, DDD base primitives & Port contracts
│   ├── Modules/
│   │   ├── Identity/
│   │   │   ├── Identity.Domain/          # User & RefreshToken entities, enum, domain errors
│   │   │   ├── Identity.Application/     # CQRS Commands/Queries, FluentValidation, Ports
│   │   │   └── Identity.Infrastructure/  # EF Core DbContext, JWT Provider, BCrypt Hasher
│   │   └── AccessControl/
│   │       ├── AccessControl.Domain/     # Role, Permission, UserRole entities & domain logic
│   │       ├── AccessControl.Application/# RBAC CQRS Commands/Queries & Validators
│   │       └── AccessControl.Infrastructure/# EF Core AccessControlDbContext & Repositories
│   └── Host/
│       └── AuthService.API/              # ASP.NET Core 8 Web API, Swagger, Middleware & Host DI
├── AuthMicroservice.sln                  # Visual Studio Solution file
├── .gitignore                            # Clean .NET 8 gitignore
└── README.md                             # Project overview & documentation
```

---

## 📚 Comprehensive Documentation

Detailed documentation is available in the [`Docs/`](Docs/) folder:

| Document | Description |
| :--- | :--- |
| [01. PRD & Requirements](Docs/01-PRD-and-Requirements.md) | Full business requirements, user stories, security policies, and non-functional requirements. |
| [02. Architecture & Hexagonal Design](Docs/02-Architecture-and-Hexagonal-Design.md) | Modular monolith architecture, boundary rules, and Hexagonal Ports & Adapters diagram. |
| [03. State & Sequence Diagrams](Docs/03-State-and-Sequence-Diagrams.md) | Mermaid sequence flows for Registration, Login, Token Refresh, and RBAC authorization. |
| [04. Database Schema & DDL](Docs/04-Database-Schema-and-DDL.md) | Complete SQL Server DDL, tables, constraints, indexes, and schema segregation rules. |
| [05. API Contracts & Swagger Spec](Docs/05-API-Contracts-and-Swagger-Spec.md) | Complete OpenAPI/Swagger 3.0 specs, sample JSON payloads, and response codes. |
| [06. Master Implementation Plan](Docs/06-Master-Implementation-Plan.md) | Phase-by-phase implementation blueprint and technical decisions. |
| [07. Agent Execution Prompts](Docs/07-Agent-Execution-Prompts.md) | Step-by-step verified execution prompts used to construct the solution. |
| [08. Complete Walkthrough & Defense Guide](Docs/08-Complete-Project-Walkthrough-and-Defense-Guide.md) | Comprehensive engineering interview defense guide, code deep-dives, and Q&A. |

---

## 🚀 Quick Start Guide

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB, SQL Express, or Docker SQL Server)

### 1. Configuration
Verify database connection and JWT signing key in [`src/Host/AuthService.API/appsettings.json`](src/Host/AuthService.API/appsettings.json):
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
  }
}
```

### 2. Build the Solution
```bash
dotnet build AuthMicroservice.sln
```

### 3. Run the Microservice
```bash
dotnet run --project src/Host/AuthService.API
```

> **Automatic Migration & Seeding:** On startup, the service automatically executes EF Core migrations for both `identity` and `rbac` schemas and seeds initial permissions, roles, and default accounts.

### 4. Access Interactive API Documentation
Open your browser and navigate to:
```
http://localhost:5000/swagger
```
*(or `https://localhost:5001/swagger` if running over HTTPS)*

---

## 🔑 Pre-Seeded Accounts & Credentials

The database is pre-seeded with two accounts for immediate testing:

| Role | Email | Password | Assigned Permissions |
| :--- | :--- | :--- | :--- |
| **SuperAdmin** | `admin@authservice.local` | `Admin@12345!` | `auth:profile:read`, `users:read`, `users:manage`, `roles:read`, `roles:write`, `roles:manage` |
| **StandardUser** | `user@authservice.local` | `User@12345!` | `auth:profile:read` |

---

## 🛡️ RBAC Permission Matrix

| Permission Code | Module Group | Description | SuperAdmin | Manager | StandardUser |
| :--- | :--- | :--- | :---: | :---: | :---: |
| `auth:profile:read` | `Identity` | Read own authenticated profile (`/me`) | ✅ | ✅ | ✅ |
| `users:read` | `AccessControl` | View users and role assignments | ✅ | ✅ | ❌ |
| `users:manage` | `AccessControl` | Assign roles & modify user status | ✅ | ❌ | ❌ |
| `roles:read` | `AccessControl` | View all roles and permissions | ✅ | ✅ | ❌ |
| `roles:write` | `AccessControl` | Create new custom roles | ✅ | ❌ | ❌ |
| `roles:manage` | `AccessControl` | Bind/unbind permissions to roles | ✅ | ❌ | ❌ |

---

## 📡 API Endpoints Reference

### Identity & Authentication (`/api/v1/auth`)
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/v1/auth/register` | Register a new user (automatically assigned `StandardUser`) | Anonymous |
| `POST` | `/api/v1/auth/login` | Authenticate with email/password; returns JWT access & refresh token | Anonymous |
| `POST` | `/api/v1/auth/refresh` | Exchange active refresh token for rotated refresh & access tokens | Anonymous |
| `POST` | `/api/v1/auth/revoke` | Revoke an active refresh token | `[Authorize]` |
| `GET` | `/api/v1/auth/me` | Read current user identity, assigned roles, and permissions | `[HasPermission("auth:profile:read")]` |

### Access Control & Dynamic RBAC (`/api/v1/rbac`)
| Method | Endpoint | Description | Permission Required |
| :--- | :--- | :--- | :---: |
| `POST` | `/api/v1/rbac/roles` | Create a new custom role | `roles:write` |
| `GET` | `/api/v1/rbac/roles` | List all roles with assigned permissions | `roles:read` |
| `GET` | `/api/v1/rbac/permissions` | List all available granular permissions | `roles:read` |
| `PUT` | `/api/v1/rbac/roles/{roleId}/permissions` | Update permissions assigned to a role | `roles:manage` |
| `POST` | `/api/v1/rbac/users/{userId}/roles` | Assign a role to a user | `users:manage` |
| `PATCH` | `/api/v1/rbac/users/{userId}/status` | Update user status (`Active`/`Suspended`/`Deactivated`) & revoke tokens | `users:manage` |

### Operational & Health Checks
| Method | Endpoint | Description | Auth Required |
| :--- | :--- | :--- | :---: |
| `GET` | `/health` | Verifies database connectivity and readiness across both DB schemas | Anonymous |

---

## 🔒 Security & Production Engineering Highlights

- **Cryptographic Refresh Token Rotation:** Every refresh operation invalidates the existing token and generates a cryptographically random replacement (`RNGCryptoServiceProvider`). Revoked or reused tokens cannot be exchanged.
- **BCrypt Password Hashing:** User passwords are encrypted with adaptive work-factor BCrypt hashing before persistence.
- **Granular Permission Authorization Handler:** Claims-based authorization using ASP.NET Core `IAuthorizationRequirement` and `AuthorizationHandler<PermissionRequirement>`.
- **Zero Cross-Schema Foreign Keys:** Strict logical boundary isolation ensuring the Identity and Access Control modules can be split into separate physical microservices with zero database refactoring.
- **Cross-Cutting Pipeline Behaviors:** MediatR validation pipeline behavior (`ValidationBehavior`) intercepting incoming commands with FluentValidation before reaching domain handlers.
- **Centralized Global Exception Handling:** Clean RFC 7807 `ProblemDetails` responses across all unhandled errors and domain validation exceptions.

---

## 🧪 Testing in Swagger UI

1. Run the API and open `http://localhost:5000/swagger`.
2. Expand `POST /api/v1/auth/login` and click **Try it out**.
3. Use the pre-seeded admin credentials:
   ```json
   {
     "email": "admin@authservice.local",
     "password": "Admin@12345!"
   }
   ```
4. Copy the `accessToken` value from the response body.
5. Click the green **Authorize** button at the top right of the Swagger UI.
6. Enter:
   ```text
   Bearer <your_copied_token>
   ```
7. Click **Authorize** and **Close**.
8. Test protected endpoints:
   - `GET /api/v1/auth/me`
   - `GET /api/v1/rbac/roles`
   - `POST /api/v1/rbac/roles`

---

## 📄 License
This project is licensed under the [MIT License](LICENSE).