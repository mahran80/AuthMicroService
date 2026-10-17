# API Contracts, JSON Payloads & Swagger Verification Specification

## 1. JWT Access Token Payload Structure[cite: 1]
When a user authenticates via `/api/v1/auth/login`, the issued JWT carries the following decoded payload structure:

```json
{
  "sub": "8f9d2a10-4b5c-4e1a-9c2d-1a2b3c4d5e6f",
  "email": "admin@authservice.local",
  "name": "System Administrator",
  "jti": "c9a12b34-5678-4321-8765-abcdef123456",
  "roles": [
    "SuperAdmin"
  ],
  "permissions": [
    "auth:profile:read",
    "users:read",
    "users:manage",
    "roles:read",
    "roles:write",
    "roles:manage"
  ],
  "iss": "AuthMicroservice",
  "aud": "EnterpriseMicroservices",
  "iat": 1760700000,
  "exp": 1760700900
}
```

---

## 2. Module 1: Identity & Authentication Endpoints (`/api/v1/auth`)

### 2.1 `POST /api/v1/auth/register`
* **Access:** `[AllowAnonymous]`
* **Request Body:**
```json
{
  "firstName": "Ahmed",
  "lastName": "Hassan",
  "email": "ahmed.hassan@example.com",
  "password": "Secure@Password123"
}
```
* **Response (`201 Created`):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "ahmed.hassan@example.com",
  "assignedDefaultRole": "StandardUser",
  "createdAtUtc": "2026-10-17T10:00:00Z"
}
```

---

### 2.2 `POST /api/v1/auth/login`[cite: 1]
* **Access:** `[AllowAnonymous]`
* **Request Body:**
```json
{
  "email": "admin@authservice.local",
  "password": "Admin@12345!"
}
```
* **Response (`200 OK`):**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "dGhpcyBpcyBhIHRlc3QgcmVmcmVzaCB0b2tlbiBzdHJpbmc=",
  "accessTokenExpiresAtUtc": "2026-10-17T10:15:00Z",
  "refreshTokenExpiresAtUtc": "2026-10-24T10:00:00Z",
  "user": {
    "id": "8f9d2a10-4b5c-4e1a-9c2d-1a2b3c4d5e6f",
    "email": "admin@authservice.local",
    "fullName": "System Administrator",
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
}
```

---

### 2.3 `POST /api/v1/auth/refresh`
* **Access:** `[AllowAnonymous]`
* **Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHRlc3QgcmVmcmVzaCB0b2tlbiBzdHJpbmc="
}
```
* **Response (`200 OK`):** Returns a new `accessToken` and rotated `refreshToken` matching the Login response format.

---

### 2.4 `POST /api/v1/auth/revoke`
* **Access:** `[Authorize]`
* **Request Body:**
```json
{
  "refreshToken": "dGhpcyBpcyBhIHRlc3QgcmVmcmVzaCB0b2tlbiBzdHJpbmc="
}
```
* **Response (`200 OK`):**
```json
{
  "message": "Refresh token has been successfully revoked."
}
```

---

### 2.5 `GET /api/v1/auth/me`
* **Access:** `[Authorize]` (Requires `auth:profile:read`)
* **Response (`200 OK`):**
```json
{
  "userId": "8f9d2a10-4b5c-4e1a-9c2d-1a2b3c4d5e6f",
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

---

## 3. Module 2: Access Control (RBAC) Endpoints (`/api/v1/rbac`)[cite: 1]

### 3.1 `POST /api/v1/rbac/roles`
* **Access:** `[HasPermission("roles:write")]`
* **Request Body:**
```json
{
  "name": "Auditor",
  "description": "Read-only access to user and role configurations."
}
```
* **Response (`201 Created`):**
```json
{
  "roleId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Auditor",
  "normalizedName": "AUDITOR",
  "description": "Read-only access to user and role configurations.",
  "isSystemRole": false
}
```

---

### 3.2 `GET /api/v1/rbac/roles`
* **Access:** `[HasPermission("roles:read")]`
* **Response (`200 OK`):**
```json
[
  {
    "roleId": "11111111-1111-1111-1111-111111111111",
    "name": "SuperAdmin",
    "isSystemRole": true,
    "permissions": ["auth:profile:read", "users:read", "users:manage", "roles:read", "roles:write", "roles:manage"]
  },
  {
    "roleId": "22222222-2222-2222-2222-222222222222",
    "name": "StandardUser",
    "isSystemRole": true,
    "permissions": ["auth:profile:read"]
  }
]
```

---

### 3.3 `GET /api/v1/rbac/permissions`
* **Access:** `[HasPermission("roles:read")]`
* **Response (`200 OK`):** Returns the complete list of seeded permissions (`Code`, `DisplayName`, `ModuleGroup`, `Description`).

---

### 3.4 `PUT /api/v1/rbac/roles/{roleId}/permissions`
* **Access:** `[HasPermission("roles:manage")]`
* **Request Body:**
```json
{
  "permissionCodes": [
    "auth:profile:read",
    "users:read",
    "roles:read"
  ]
}
```
* **Response (`200 OK`):**
```json
{
  "roleId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "roleName": "Auditor",
  "updatedPermissions": [
    "auth:profile:read",
    "users:read",
    "roles:read"
  ]
}
```

---

### 3.5 `POST /api/v1/rbac/users/{userId}/roles`
* **Access:** `[HasPermission("users:manage")]`
* **Request Body:**
```json
{
  "roleId": "7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```
* **Response (`200 OK`):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assignedRoleId": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "assignedRoleName": "Auditor"
}
```

---

### 3.6 `PATCH /api/v1/rbac/users/{userId}/status`
* **Access:** `[HasPermission("users:manage")]`
* **Request Body:**
```json
{
  "status": "Suspended"
}
```
* **Response (`200 OK`):**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "newStatus": "Suspended",
  "revokedActiveTokensCount": 1
}
```

---

## 4. Standardized Error Contract (RFC 7807 `ProblemDetails`)
All `400`, `401`, `403`, `404`, `409`, and `500` responses return the following unified JSON structure:

```json
{
  "type": "[https://tools.ietf.org/html/rfc7231#section-6.5.1](https://tools.ietf.org/html/rfc7231#section-6.5.1)",
  "title": "Validation Failure",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/auth/register",
  "errorCode": "Identity.ValidationError",
  "errors": {
    "Email": ["Email address is already registered."],
    "Password": ["Password must contain at least one special character."]
  }
}
```