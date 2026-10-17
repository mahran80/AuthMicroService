# System Behavior: State Machines & Sequence Workflows

## 1. State Machine Diagrams

### 1.1 User Account Lifecycle State Diagram
This state machine governs the `User.Status` property inside the `Identity.Domain` layer. Only users in the `Active` state can authenticate or refresh tokens.

```mermaid
stateDiagram-v2
    [*] --> Active : User Registers (FR-AUTH-01)\n[Default Role: StandardUser]

    Active --> Suspended : Admin Suspends User (FR-RBAC-05)\n[Action: Revoke All Active Refresh Tokens]
    Suspended --> Active : Admin Reactivates User (FR-RBAC-05)

    Active --> Deactivated : Admin / User Deactivates Account\n[Action: Revoke All Active Refresh Tokens]
    Suspended --> Deactivated : Admin Permanently Deactivates Account

    Deactivated --> [*] : Terminal State (Cannot Authenticate)
```

---

### 1.2 Refresh Token Lifecycle State Diagram
This state machine governs the security lifecycle of a `RefreshToken` entity to prevent replay attacks via **Refresh Token Rotation**.

```mermaid
stateDiagram-v2
    [*] --> Active : Issued on Login or Refresh\n(RevokedAtUtc == null && Now < ExpiresAtUtc)

    Active --> Rotated : Client Calls POST /api/v1/auth/refresh\n[Action: Set RevokedAtUtc, Set ReplacedByToken,\nIssue New Token Pair]
    
    Active --> Revoked : User Calls POST /api/v1/auth/revoke (Logout)\nOR Admin Suspends User\n[Action: Set RevokedAtUtc, Set ReasonRevoked]
    
    Active --> Expired : Time Passes (UtcNow >= ExpiresAtUtc)

    Rotated --> [*] : Terminal State (Replay Attempt Triggers Security Alert)
    Revoked --> [*] : Terminal State (Returns 401 Unauthorized)
    Expired --> [*] : Terminal State (Returns 401 Unauthorized)
```

---

## 2. Sequence Diagrams

### 2.1 User Registration & Cross-Module Default Role Assignment

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client (Swagger UI)
    participant API as AuthController
    participant Handler as RegisterUserCommandHandler
    participant Hasher as IPasswordHasherPort (BCrypt)
    participant UserRepo as IUserRepository (SQL [identity])
    participant RbacPort as IRbacAccessPort (Adapter)
    participant RbacDB as AccessControlDbContext (SQL [rbac])

    Client->>API: POST /api/v1/auth/register
    API->>Handler: Send(RegisterUserCommand)
    Handler->>UserRepo: ExistsByEmailAsync(email)
    UserRepo-->>Handler: false
    Handler->>Hasher: HashPassword(plainPassword)
    Hasher-->>Handler: passwordHash
    Handler->>UserRepo: AddAsync(new User)
    UserRepo-->>Handler: User Persisted
    Handler->>RbacPort: AssignDefaultRoleAsync(user.Id, "StandardUser")
    RbacPort->>RbacDB: Insert UserRole (UserId, StandardUserRoleId)
    RbacDB-->>RbacPort: Success
    RbacPort-->>Handler: Success
    Handler-->>API: Result<RegisterUserResponse> (201 Created)
    API-->>Client: 201 Created (UserId, Email)
```

---

### 2.2 Login, Cross-Module Claim Aggregation & JWT Issuance[cite: 1]

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client (Swagger UI)
    participant API as AuthController
    participant Handler as LoginCommandHandler
    participant UserRepo as IUserRepository (SQL [identity])
    participant Hasher as IPasswordHasherPort
    participant RbacPort as IRbacAccessPort
    participant JwtPort as IJwtProviderPort

    Client->>API: POST /api/v1/auth/login (Email, Password)
    API->>Handler: Send(LoginCommand)
    Handler->>UserRepo: GetByEmailAsync(email)
    UserRepo-->>Handler: User Entity
    Handler->>Handler: Check User.Status == Active
    Handler->>Hasher: VerifyPassword(password, user.PasswordHash)
    Hasher-->>Handler: true
    Handler->>RbacPort: GetUserAccessProfileAsync(user.Id)
    RbacPort-->>Handler: Roles ["SuperAdmin"], Permissions ["roles:manage", ...]
    Handler->>JwtPort: GenerateTokenPair(User, Roles, Permissions)
    JwtPort-->>Handler: AccessToken (JWT) + RefreshToken Entity
    Handler->>UserRepo: SaveRefreshTokenAsync(RefreshToken)
    Handler-->>API: Result<AuthTokenResponse>
    API-->>Client: 200 OK (AccessToken, RefreshToken, ExpiresIn)
```

---

### 2.3 Policy-Based RBAC Authorization Flow[cite: 1]

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client (Swagger UI)
    participant MW as JwtBearerMiddleware
    participant AuthZ as PermissionAuthorizationHandler
    participant API as RbacController
    participant Handler as CreateRoleCommandHandler

    Client->>MW: POST /api/v1/rbac/roles (Authorization: Bearer <JWT>)
    MW->>MW: Validate Signature, Issuer, Audience, Lifetime
    MW->>AuthZ: Invoke Policy Check [HasPermission("roles:write")]
    AuthZ->>AuthZ: Inspect User.Claims.Where(c => c.Type == "permissions")
    alt Permission Claim "roles:write" Exists
        AuthZ-->>API: Authorization Succeeded
        API->>Handler: Send(CreateRoleCommand)
        Handler-->>API: 201 Created (RoleDto)
        API-->>Client: 201 Created
    else Permission Claim Missing
        AuthZ-->>Client: 403 Forbidden (ProblemDetails)
    end
```