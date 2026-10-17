# Enterprise Authentication & Role-Based Access Control (RBAC) Microservice

Production-ready, highly secure, modular Authentication and Dynamic RBAC Authorization Microservice built with **.NET 8**, **Clean Architecture**, **Hexagonal Ports & Adapters**, and **CQRS**.

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
The database is partitioned into two isolated schemas with zero cross-schema database foreign keys:
1. **`identity` Schema:**
   - `identity.Users` (UUID primary key, email unique index, BCrypt password hash, user status)
   - `identity.RefreshTokens` (Cryptographic refresh tokens with rotation and revocation)
2. **`rbac` Schema:**
   - `rbac.Roles` (System roles and dynamic custom roles)
   - `rbac.Permissions` (Granular permission catalog)
   - `rbac.RolePermissions` (Role-to-permission mapping)
   - `rbac.UserRoles` (User-to-role assignment using unconstrained UUIDs across boundary)

### Cross-Module Communication (Hexagonal Ports)
- Modules interact strictly via Outbound Ports defined in `SharedKernel.Contracts`:
  - `IRbacAccessPort`: Used by Identity to fetch user roles/permissions and assign default roles.
  - `IUserIdentityPort`: Used by AccessControl to verify user existence and cascade revocation of active tokens.

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server LocalDB or full SQL Server instance

### Configuration (`appsettings.json`)
The connection string and JWT signing parameters are configured in `src/Host/AuthService.API/appsettings.json`:
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

### Running Locally
1. Navigate to the solution root and build:
   ```bash
   dotnet build AuthMicroservice.sln
   ```
2. Run the host API:
   ```bash
   dotnet run --project src/Host/AuthService.API
   ```
3. Open the Swagger UI in your browser:
   ```
   https://localhost:5001/swagger (or http://localhost:5000/swagger)
   ```

*Note: On first startup, `DatabaseSeeder` automatically applies pending EF Core migrations to SQL Server and seeds the permission catalog, default roles, and default user accounts.*

---

## 🔑 Pre-Seeded Accounts & Credentials

The database is pre-seeded with two default accounts:

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
- `POST /api/v1/auth/register` — Register a new user (assigns default `StandardUser` role).
- `POST /api/v1/auth/login` — Authenticate with email/password; returns JWT access token and rotated refresh token.
- `POST /api/v1/auth/refresh` — Exchange active refresh token for a new access token and rotated refresh token.
- `POST /api/v1/auth/revoke` — Revoke an active refresh token (`[Authorize]`).
- `GET /api/v1/auth/me` — Read current user claims and permissions (`[HasPermission("auth:profile:read")]`).

### Access Control & RBAC (`/api/v1/rbac`)
- `POST /api/v1/rbac/roles` — Create a custom role (`[HasPermission("roles:write")]`).
- `GET /api/v1/rbac/roles` — List all roles with their assigned permissions (`[HasPermission("roles:read")]`).
- `GET /api/v1/rbac/permissions` — List all granular permissions (`[HasPermission("roles:read")]`).
- `PUT /api/v1/rbac/roles/{roleId}/permissions` — Update permissions for a role (`[HasPermission("roles:manage")]`).
- `POST /api/v1/rbac/users/{userId}/roles` — Assign a role to a user (`[HasPermission("users:manage")]`).
- `PATCH /api/v1/rbac/users/{userId}/status` — Update user status (`Active`, `Suspended`, `Deactivated`) and revoke active tokens (`[HasPermission("users:manage")]`).

### System Health
- `GET /health` — Health check endpoint verifying database connectivity for both `IdentityDb` and `AccessControlDb`.

---

## 🧪 Testing in Swagger UI

1. Open `/swagger` in your browser.
2. Call `POST /api/v1/auth/login` with `admin@authservice.local` / `Admin@12345!`.
3. Copy the returned `accessToken` string.
4. Click the green **Authorize** button at the top right of the Swagger UI.
5. In the input box, type:
   ```
   Bearer <paste_jwt_token_here>
   ```
6. Click **Authorize** and then **Close**.
7. Execute protected endpoints such as `GET /api/v1/auth/me` or `GET /api/v1/rbac/roles`.