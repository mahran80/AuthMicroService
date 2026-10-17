# Product Requirements Document (PRD) & System Requirements Specification (SRS)

**Project Name:** Enterprise Auth & RBAC Microservice[cite: 1]  
**Tech Stack:** .NET 8 (C#), ASP.NET Core Web API, Entity Framework Core, SQL Server, Swagger (OpenAPI)[cite: 1]  
**Architectural Pattern:** Modular Microservice with Hexagonal Architecture (Ports & Adapters) and Clean Architecture per Module[cite: 1]  

---

## 1. Business Overview & Objective
The objective of this project is to build a standalone, production-grade **Authentication and Authorization (IAM) Microservice**[cite: 1]. It serves as the centralized security authority responsible for managing user identities, issuing stateless JSON Web Tokens (JWT), handling stateful refresh token rotation, and enforcing fine-grained Role-Based Access Control (RBAC) across modules[cite: 1]. All APIs are documented and verified directly through Swagger UI[cite: 1].

---

## 2. System Actors
| Actor ID | Actor Name | Description |
| :--- | :--- | :--- |
| **ACT-01** | **Anonymous Client** | Unauthenticated user interacting via Swagger UI to register, log in, or refresh an expired access token[cite: 1]. |
| **ACT-02** | **Authenticated User** | A verified user holding a valid `Bearer JWT` who can inspect their identity claims or revoke their active session[cite: 1]. |
| **ACT-03** | **RBAC Administrator** | A privileged user holding administrative permissions (`roles:manage`, `users:manage`) capable of managing roles, permissions, and user assignments[cite: 1]. |

---

## 3. Functional Requirements (FR)

### 3.1 Module 1: Identity & Authentication (`Identity` Module)
| Req ID | Feature | User Story / Business Rule | Acceptance Criteria |
| :--- | :--- | :--- | :--- |
| **FR-AUTH-01** | **User Registration** | As an anonymous client, I want to register a new account so that I can access the system. | - Email must be unique (case-insensitive) and valid.<br>- Password must be at least 8 characters (uppercase, lowercase, digit, special char).<br>- Password must be hashed using `BCrypt` (Work Factor >= 11).<br>- User is automatically assigned the default `StandardUser` role via the RBAC port. |
| **FR-AUTH-02** | **Login & Token Issuance** | As a registered user, I want to authenticate with my email and password to receive a JWT and Refresh Token[cite: 1]. | - Validates credentials against `BCrypt` hash.<br>- Verifies `User.Status == Active`.<br>- Queries `RBAC Module` via `IRbacIntegrationPort` to fetch user's assigned Roles and Permissions[cite: 1].<br>- Issues a short-lived JWT (15 mins) containing `sub`, `email`, `jti`, `roles[]`, and `permissions[]` claims[cite: 1].<br>- Issues and persists a 7-day cryptographic Refresh Token. |
| **FR-AUTH-03** | **Refresh Token Rotation** | As a client, I want to exchange an expired JWT and a valid Refresh Token for a new token pair. | - Verifies Refresh Token exists, is not expired, and `RevokedAtUtc == null`.<br>- Marks the old Refresh Token as revoked (`ReasonRevoked = "Rotated"`) and links `ReplacedByToken`.<br>- Issues a new JWT and a new Refresh Token. |
| **FR-AUTH-04** | **Token Revocation (Logout)** | As an authenticated user, I want to revoke my active Refresh Token to terminate my session. | - Marks the target Refresh Token's `RevokedAtUtc` with the current UTC timestamp.<br>- Subsequent refresh attempts using this token must return `401 Unauthorized`. |
| **FR-AUTH-05** | **Current Identity Context (`/me`)** | As an authenticated user, I want to view my current identity profile and active permissions[cite: 1]. | - Extracts `UserId`, `Email`, `Roles`, and `Permissions` directly from the validated JWT claims[cite: 1]. |

### 3.2 Module 2: Role-Based Access Control (`AccessControl` Module)[cite: 1]
| Req ID | Feature | User Story / Business Rule | Acceptance Criteria |
| :--- | :--- | :--- | :--- |
| **FR-RBAC-01** | **Create Role** | As an Admin, I want to create custom roles (e.g., `Manager`, `Auditor`)[cite: 1]. | - Requires `roles:write` permission.<br>- Role `Name` and `NormalizedName` must be unique.<br>- Custom roles have `IsSystemRole = false`. |
| **FR-RBAC-02** | **Query Roles & Permissions** | As an Admin/Manager, I want to list all roles and the catalog of system permissions[cite: 1]. | - Requires `roles:read` permission.<br>- Returns all roles with their bound permission codes. |
| **FR-RBAC-03** | **Bind Permissions to Role** | As an Admin, I want to assign or update permissions for a specific role[cite: 1]. | - Requires `roles:manage` permission.<br>- Validates all submitted permission codes exist in the catalog.<br>- Prevents stripping core permissions from `SuperAdmin` (`IsSystemRole == true`). |
| **FR-RBAC-04** | **Assign Role to User** | As an Admin, I want to assign a role to a specific user[cite: 1]. | - Requires `users:manage` permission.<br>- Verifies the target `UserId` exists in the `Identity` module via an integration port.<br>- Idempotent assignment (no duplicate `UserId + RoleId` pairs). |
| **FR-RBAC-05** | **User Status Management** | As an Admin, I want to suspend or reactivate a user account. | - Requires `users:manage` permission.<br>- If set to `Suspended`, all active Refresh Tokens for that user are immediately revoked. |

---

## 4. Non-Functional Requirements (NFR)
| NFR ID | Category | Requirement Specification |
| :--- | :--- | :--- |
| **NFR-ARCH-01** | **Modular Isolation** | The solution must be divided into independent modules (`Identity` and `AccessControl`)[cite: 1]. Direct database joins or entity references across modules are strictly forbidden. Cross-module calls must use Inbound/Outbound Ports[cite: 1]. |
| **NFR-ARCH-02** | **Clean Architecture** | Every module must strictly separate `Domain`, `Application`, and `Infrastructure`[cite: 1]. The `Domain` layer must have zero external NuGet dependencies. |
| **NFR-SEC-01** | **Stateless RBAC Enforcement** | Endpoint authorization must evaluate custom `[HasPermission("...")]` attributes against JWT claims without querying SQL Server on every HTTP request[cite: 1]. |
| **NFR-API-01** | **RFC 7807 Error Standard** | All domain, validation, and security exceptions must be caught by a Global Exception Handler and returned as standardized `ProblemDetails` JSON payloads. |
| **NFR-DOC-01** | **Swagger UI Readiness** | Swagger must be pre-configured with OpenAPI `Bearer` security definitions so testers can authenticate and test RBAC-protected endpoints directly in the browser[cite: 1]. |

---

## 5. RBAC Permission Matrix & Default Seed Data[cite: 1]
The microservice must automatically apply EF Core migrations and seed the following catalog upon startup:

### 5.1 Permission Catalog
| Permission Code | Module Group | Description |
| :--- | :--- | :--- |
| `auth:profile:read` | `Identity` | Read own authenticated profile (`/api/v1/auth/me`) |
| `users:read` | `AccessControl` | View users and their assigned roles/permissions |
| `users:manage` | `AccessControl` | Assign roles to users and change user account status |
| `roles:read` | `AccessControl` | View all roles and the permission catalog |
| `roles:write` | `AccessControl` | Create new custom roles |
| `roles:manage` | `AccessControl` | Bind/unbind permissions to roles |

### 5.2 Default Roles Matrix
| Permission Code | `SuperAdmin` (System Role) | `Manager` (System Role) | `StandardUser` (Default Role) |
| :--- | :---: | :---: | :---: |
| `auth:profile:read` | ✅ | ✅ | ✅ |
| `users:read` | ✅ | ✅ | ❌ |
| `users:manage` | ✅ | ❌ | ❌ |
| `roles:read` | ✅ | ✅ | ❌ |
| `roles:write` | ✅ | ❌ | ❌ |
| `roles:manage` | ✅ | ❌ | ❌ |

### 5.3 Default Seeded User Accounts
1. **Super Admin:** `admin@authservice.local` | Password: `Admin@12345!` (Assigned `SuperAdmin` role)
2. **Standard User:** `user@authservice.local` | Password: `User@12345!` (Assigned `StandardUser` role)