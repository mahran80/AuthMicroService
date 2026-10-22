# Ultra-Detailed Step-by-Step Unification Prompts (Strict Mode for Weak Models)

**CRITICAL INSTRUCTION FOR USER:** Send **ONLY ONE prompt at a time** to the AI Agent, and append the **MANDATORY POST-PHASE AUDIT REPORT** block at the end of every prompt.

---

## 🔵 PROMPT U1: Execute Phase U1 (Modular Presentation Layer Extraction)

```text
You are executing Phase U1 of our Team Unification Plan.
Read `Docs/08-Team-Unification-Master-Plan.md` before starting.

### OBJECTIVE
Extract the REST Controllers and Presentation concerns out of `AuthService.API` into dedicated `.Presentation` Class Library projects inside each module (`Identity.Presentation` and `AccessControl.Presentation`) to enforce strict Modular Clean Architecture, while keeping `AuthService.API` purely as the Host/Bootstrapper.

### STRICT CONSTRAINTS
1. Execute ONLY Phase U1. Do NOT touch `Raneem/` or `Marwan/` folders yet.
2. All projects MUST target `net8.0`.
3. Do NOT add deprecated `Microsoft.AspNetCore.Mvc` NuGet packages. Instead, use `<FrameworkReference Include="Microsoft.AspNetCore.App"/>` inside `<ItemGroup>` for `.Presentation` projects and `SharedKernel`.

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Update `SharedKernel` to Host Shared Presentation Primitives
1. Open `src/Shared/SharedKernel/SharedKernel.csproj` and add:
   ```xml
   <ItemGroup>
     <FrameworkReference Include="Microsoft.AspNetCore.App" />
   </ItemGroup>
   ```
2. Move `HasPermissionAttribute.cs` from `src/Host/AuthService.API/Authorization/HasPermissionAttribute.cs` to `src/Shared/SharedKernel/Presentation/HasPermissionAttribute.cs` under namespace `SharedKernel.Presentation`:
   - Keep `public sealed class HasPermissionAttribute : AuthorizeAttribute` with constructor `public HasPermissionAttribute(string permission) : base(policy: permission) { }`.
3. Move `ResultExtensions.cs` from `src/Host/AuthService.API/Extensions/ResultExtensions.cs` to `src/Shared/SharedKernel/Presentation/ResultExtensions.cs` under namespace `SharedKernel.Presentation`:
   - Ensure `ToProblemDetails(this Result result, HttpContext httpContext)` maps `ErrorType.Validation` (400), `ErrorType.Unauthorized` (401), `ErrorType.Forbidden` (403), `ErrorType.NotFound` (404), `ErrorType.Conflict` (409), and `ErrorType.Failure` (500) into RFC 7807 `ProblemDetails` with `extensions["errorCode"]` and `extensions["errors"]`.
4. Delete the old `HasPermissionAttribute.cs` and `ResultExtensions.cs` from `AuthService.API`.

#### Step 2: Create `Identity.Presentation` Project
1. Create directory and project `src/Modules/Identity/Identity.Presentation/Identity.Presentation.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net8.0</TargetFramework>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
     </PropertyGroup>
     <ItemGroup>
       <FrameworkReference Include="Microsoft.AspNetCore.App" />
     </ItemGroup>
     <ItemGroup>
       <ProjectReference Include="..\Identity.Application\Identity.Application.csproj" />
       <ProjectReference Include="..\..\..\Shared\SharedKernel\SharedKernel.csproj" />
     </ItemGroup>
   </Project>
   ```
2. Add `Identity.Presentation.csproj` to `AuthMicroservice.sln`.
3. Move `src/Host/AuthService.API/Controllers/AuthController.cs` to `src/Modules/Identity/Identity.Presentation/Controllers/AuthController.cs`.
   - Change namespace to `Identity.Presentation.Controllers`.
   - Update `using` directives to include `SharedKernel.Presentation`.
   - Delete `src/Host/AuthService.API/Controllers/AuthController.cs`.

#### Step 3: Create `AccessControl.Presentation` Project
1. Create directory and project `src/Modules/AccessControl/AccessControl.Presentation/AccessControl.Presentation.csproj`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <TargetFramework>net8.0</TargetFramework>
       <ImplicitUsings>enable</ImplicitUsings>
       <Nullable>enable</Nullable>
     </PropertyGroup>
     <ItemGroup>
       <FrameworkReference Include="Microsoft.AspNetCore.App" />
     </ItemGroup>
     <ItemGroup>
       <ProjectReference Include="..\AccessControl.Application\AccessControl.Application.csproj" />
       <ProjectReference Include="..\..\..\Shared\SharedKernel\SharedKernel.csproj" />
     </ItemGroup>
   </Project>
   ```
2. Add `AccessControl.Presentation.csproj` to `AuthMicroservice.sln`.
3. Move `src/Host/AuthService.API/Controllers/RbacController.cs` to `src/Modules/AccessControl/AccessControl.Presentation/Controllers/RbacController.cs`.
   - Change namespace to `AccessControl.Presentation.Controllers`.
   - Update `using` directives to include `SharedKernel.Presentation`.
   - Delete `src/Host/AuthService.API/Controllers/RbacController.cs`.

#### Step 4: Wire Application Parts in `AuthService.API`
1. Add `<ProjectReference>` in `src/Host/AuthService.API/AuthService.API.csproj` pointing to `Identity.Presentation.csproj` and `AccessControl.Presentation.csproj`.
2. In `src/Host/AuthService.API/Program.cs`, update `builder.Services.AddControllers()` to explicitly register the modular presentation assemblies:
   ```csharp
   builder.Services.AddControllers()
       .AddApplicationPart(typeof(Identity.Presentation.Controllers.AuthController).Assembly)
       .AddApplicationPart(typeof(AccessControl.Presentation.Controllers.RbacController).Assembly);
   ```
3. Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```

---

## 🔵 PROMPT U2: Execute Phase U2 (Scaffold & Port `Users` Module from Teammate's Code)

```text
You are executing Phase U2 of our Team Unification Plan (Porting the Third Teammate's `Users` Module).

### MANDATORY PRE-READING
Before writing any code, inspect the teammate's actual files in `Raneem/users/dotnet/`:
1. `Raneem/users/dotnet/domain/entities/User.cs`, `phone.cs`, `email.cs`, `SetPasswordToken.cs`
2. `Raneem/users/dotnet/application/commands/CreateUserCommand.cs`, `UpdateUserCommand.cs`, `DeleteUserCommand.cs`
3. `Raneem/users/dotnet/application/queries/GetUserByIdQuery.cs`, `GetUserByPhoneQuery.cs`
4. `Raneem/users/dotnet/infrastructure/Persistence/UsersDbContext.cs`, `Repositories/UserRepository.cs`
5. `Raneem/users/dotnet/presentation/rest/UsersController.cs` and `presentation/grpc/UserGrpcService.cs`

### ARCHITECTURAL FLAWS IN TEAMMATE'S CODE THAT YOU MUST FIX
1. **Fix Cross-Module Database Hijacking:** In `Raneem/users/dotnet/infrastructure/Persistence/UsersDbContext.cs`, the teammate mapped `IAM_Authentication_Email` and `IAM_Authentication_Phone` (tables owned by Auth!) directly inside `UsersDbContext` with cascade foreign keys. You MUST NOT touch `[identity]` tables from `UsersDbContext`. Instead, `UsersDbContext` must use its own isolated schema `"users"` with table `[users].[UserProfiles]`.
2. **Fix Anemic User Entity:** In the teammate's `User.cs`, `User` only had `UserId`, `IsDeleted`, `DeletedAt`, and stored the user's `Name` inside the `Phone` table! You must consolidate `UserId`, `FullName`, `Email`, `CountryCode`, `PhoneNumber`, `IsDeleted`, `DeletedAtUtc`, `CreatedAtUtc`, and `UpdatedAtUtc` into a single rich entity `UserProfile : Entity` in `Users.Domain`.
3. **Fix Abandoned REST Controller:** The teammate only exposed `POST api/users` in `UsersController.cs` and left `GetById`, `GetByPhone`, `UpdateUser`, and `DeleteUser` only in `UserGrpcService.cs`. You MUST expose ALL 5 operations as clean REST endpoints in `Users.Presentation/Controllers/UsersController.cs` so they can be tested in Swagger.
4. **Fix Raw Exception Throwing:** Replace all `throw new Exception("...")` in the teammate's commands with `Result<T>` and typed `UsersErrors`.

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Scaffold the 4 `Users` Module Projects (`net8.0`)
Create under `src/Modules/Users/` and add to `AuthMicroservice.sln`:
1. `Users.Domain/Users.Domain.csproj`: References ONLY `SharedKernel.csproj` (0 NuGet packages).
2. `Users.Application/Users.Application.csproj`: References `Users.Domain.csproj`, `SharedKernel.csproj` + Package `FluentValidation.DependencyInjectionExtensions` (11.10.0).
3. `Users.Infrastructure/Users.Infrastructure.csproj`: References `Users.Application.csproj`, `SharedKernel.csproj` + Package `Microsoft.EntityFrameworkCore.SqlServer` (8.0.10).
4. `Users.Presentation/Users.Presentation.csproj`: References `Users.Application.csproj`, `SharedKernel.csproj` + `<FrameworkReference Include="Microsoft.AspNetCore.App"/>`.

#### Step 2: Update Cross-Module Hexagonal Ports in `SharedKernel.Contracts`
1. Create `src/Shared/SharedKernel/Contracts/IUserProfilePort.cs`:
   ```csharp
   using SharedKernel.Application;
   namespace SharedKernel.Contracts;

   public interface IUserProfilePort
   {
       Task<Result> CreateOrSyncProfileAsync(
           Guid userId,
           string email,
           string fullName,
           string? countryCode,
           string? phoneNumber,
           CancellationToken cancellationToken = default);

       Task<Result> SoftDeleteProfileAsync(Guid userId, CancellationToken cancellationToken = default);
   }
   ```
2. Update `src/Shared/SharedKernel/Contracts/IUserIdentityPort.cs` to add admin user provisioning support (porting the teammate's `CreateUserCommand` magic-link onboarding flow):
   ```csharp
   Task<Result<Guid>> ProvisionUserWithoutPasswordAsync(
       string email,
       string firstName,
       string lastName,
       string? countryCode,
       string? phoneNumber,
       CancellationToken cancellationToken = default);

   Task<Result> UpdateUserContactInIdentityAsync(
       Guid userId,
       string email,
       string firstName,
       string lastName,
       string? countryCode,
       string? phoneNumber,
       CancellationToken cancellationToken = default);
   ```
3. Implement those 2 new methods inside `Identity.Infrastructure/Integration/UserIdentityAdapter.cs`:
   - `ProvisionUserWithoutPasswordAsync`: Checks email uniqueness in `IUserRepository`, creates a `User` with a random unguessable placeholder password hash, saves to `[identity].[Users]`, assigns `"StandardUser"` via `IRbacAccessPort`, and returns `Result.Success(user.Id)`.
   - `UpdateUserContactInIdentityAsync`: Finds user by `userId` in `IUserRepository`, updates email/name, and saves changes.
4. Update `RegisterUserCommandHandler.cs` in `Identity.Application` to inject `IUserProfilePort` and call `await _userProfilePort.CreateOrSyncProfileAsync(user.Id, user.Email, $"{user.FirstName} {user.LastName}".Trim(), null, null, cancellationToken);` after user registration so `[users].[UserProfiles]` stays synchronized automatically!

#### Step 3: Implement `Users.Domain`
1. `Entities/UserProfile.cs`: Inherits `Entity` (`Id` = `UserId`).
   - Properties: `Email` (string), `FullName` (string), `CountryCode` (string?), `PhoneNumber` (string?), `IsDeleted` (bool), `DeletedAtUtc` (DateTime?), `CreatedAtUtc` (DateTime), `UpdatedAtUtc` (DateTime?).
   - Static factory `Create(Guid userId, string email, string fullName, string? countryCode, string? phoneNumber)`.
   - Methods: `UpdateDetails(string fullName, string email, string? countryCode, string? phoneNumber)`, `SoftDelete()`, `Restore()`.
2. `Errors/UsersErrors.cs`:
   - `ProfileNotFound` (NotFound 404), `EmailAlreadyExists` (Conflict 409), `PhoneAlreadyExists` (Conflict 409), `ProfileAlreadyDeleted` (Conflict 409).

#### Step 4: Implement `Users.Application` (Porting Teammate's Commands & Queries)
1. `Abstractions/Ports/IUserProfileRepository.cs`:
   - `GetByIdAsync(Guid userId, CancellationToken ct)`
   - `GetByPhoneAsync(string? countryCode, string phoneNumber, CancellationToken ct)`
   - `GetByEmailAsync(string email, CancellationToken ct)`
   - `GetAllActiveAsync(CancellationToken ct)`
   - `ExistsByEmailAsync(string email, Guid? excludeUserId, CancellationToken ct)`
   - `ExistsByPhoneAsync(string? countryCode, string phoneNumber, Guid? excludeUserId, CancellationToken ct)`
   - `AddAsync(UserProfile profile, CancellationToken ct)`
   - `SaveChangesAsync(CancellationToken ct)`
2. `DTOs/UserProfileDtos.cs`:
   - `UserProfileResponse(Guid UserId, string FullName, string Email, string? CountryCode, string? PhoneNumber, bool IsDeleted, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc)`
3. Implement CQRS Features (Commands, Validators, Queries, Handlers):
   - `Features/CreateUser/CreateUserCommand.cs` (`string FullName, string Email, string? CountryCode, string? PhoneNumber`) -> Ported from teammate's `CreateUserCommand.cs`: validates email & phone uniqueness, calls `IUserIdentityPort.ProvisionUserWithoutPasswordAsync`, creates `UserProfile`, saves, and returns `UserProfileResponse`.
   - `Features/UpdateUser/UpdateUserCommand.cs` (`Guid UserId, string FullName, string Email, string? CountryCode, string? PhoneNumber`) -> Ported from teammate's `UpdateUserCommand.cs`: validates uniqueness excluding current `UserId`, updates `UserProfile`, syncs via `IUserIdentityPort.UpdateUserContactInIdentityAsync`.
   - `Features/DeleteUser/DeleteUserCommand.cs` (`Guid UserId`) -> Ported from teammate's `DeleteUserCommand.cs`: marks `profile.SoftDelete()`, calls `IUserIdentityPort.UpdateUserStatusAndRevokeTokensAsync(userId, "Deactivated")` to revoke all tokens and deactivate credentials!
   - `Features/GetUserById/GetUserByIdQuery.cs` (`Guid UserId`) -> Ported from teammate's `GetUserByIdQuery.cs`.
   - `Features/GetUserByPhone/GetUserByPhoneQuery.cs` (`string? CountryCode, string PhoneNumber`) -> Ported from teammate's `GetUserByPhoneQuery.cs`.
   - `Features/GetAllUsers/GetAllUsersQuery.cs` -> Lists all active user profiles.
4. `DependencyInjection.cs`: `AddUsersApplication(this IServiceCollection services)`.

#### Step 5: Implement `Users.Infrastructure`
1. `Persistence/UsersDbContext.cs`: Default schema `"users"`. `DbSet<UserProfile> UserProfiles`.
2. `Persistence/Configurations/UserProfileConfiguration.cs`: Table `"UserProfiles"` in schema `"users"`. PK `Id` mapped to column `"UserId"`. Unique index `UX_UserProfiles_Email`, filtered unique index `UX_UserProfiles_Phone` on `(CountryCode, PhoneNumber)` where `PhoneNumber IS NOT NULL`.
3. `Persistence/Repositories/UserProfileRepository.cs`: Implements `IUserProfileRepository`.
4. `Integration/UserProfileAdapter.cs`: Implements `SharedKernel.Contracts.IUserProfilePort`.
5. `DependencyInjection.cs`: `AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)`.

#### Step 6: Implement `Users.Presentation` & Wire in `AuthService.API`
1. `Controllers/UsersController.cs` under route `api/v1/users`:
   - `POST /api/v1/users` (`[HasPermission("users:manage")]`) -> `CreateUserCommand`
   - `GET /api/v1/users` (`[HasPermission("users:read")]`) -> `GetAllUsersQuery`
   - `GET /api/v1/users/{userId:guid}` (`[HasPermission("users:read")]`) -> `GetUserByIdQuery`
   - `GET /api/v1/users/by-phone` (`[HasPermission("users:read")]`) -> `GetUserByPhoneQuery`
   - `PUT /api/v1/users/{userId:guid}` (`[HasPermission("users:manage")]`) -> `UpdateUserCommand`
   - `DELETE /api/v1/users/{userId:guid}` (`[HasPermission("users:manage")]`) -> `DeleteUserCommand`
2. Register `AddUsersApplication()`, `AddUsersInfrastructure(builder.Configuration)`, and `.AddApplicationPart(typeof(UsersController).Assembly)` in `AuthService.API/Program.cs`.
3. Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```

---

## 🔵 PROMPT U3: Execute Phase U3 (Port Raneem's WhatsApp OTP, SMTP, LoginSession & Password Reset)

```text
You are executing Phase U3 of our Team Unification Plan (Porting Raneem's Auth features into `Identity`).

### MANDATORY PRE-READING
Inspect Raneem's actual source files in `Raneem/Auth/dotnet/`:
1. `Raneem/Auth/dotnet/infrastructure/UltraMsgWhatsAppService.cs`
2. `Raneem/Auth/dotnet/infrastructure/SmtpEmailService.cs`
3. `Raneem/Auth/dotnet/application/commands/RegisterCommand.cs`, `LoginMobileCommand.cs`, `VerifyOtpCommand.cs`, `ResendOtpCommand.cs`, `ForgotPasswordCommand.cs`, `ChangePasswordCommand.cs`
4. `Raneem/Auth/dotnet/domain/entities/Otp.cs`, `LoginSession.cs`, `PasswordResetToken.cs`, `SetPasswordToken.cs`

### CRITICAL FLAWS & VULNERABILITIES IN RANEEM'S CODE YOU MUST FIX
1. **Fix Critical OTP Leak in HTTP Response & Console:** In Raneem's `RegisterCommand.cs` (lines 351-356) and `RegisterController.cs`, she wrote `Console.WriteLine(result.OtpCode)` and returned `OtpCode` inside the JSON response `{ Message, OtpCode }`. You MUST NEVER include `OtpCode` in any Response DTO or log it to Console in plain text when sending to production clients!
2. **Fix Plaintext OTP & Reset Token Storage in Database:** Raneem stored `Otp.Code` and `PasswordResetToken.Token` as plaintext strings in SQL Server. You MUST hash all OTP codes and reset tokens using `SHA256` before storing them in `CodeHash` / `TokenHash` columns (matching Marwan's secure `OtpHasher` approach).
3. **Fix 2-Minute Hardcoded Expiry & Hardcoded Localhost URL:** Raneem hardcoded `DateTime.UtcNow.AddMinutes(2)` for password reset and hardcoded `http://localhost:4200/#/set-password?token={token}`. You must set OTP expiry to 5 minutes, Password Reset token expiry to 15 minutes (or 24 hours for Admin Invite tokens), and read `Frontend:ResetPasswordUrl` from `IConfiguration` (with fallback default `"http://localhost:4200/#/set-password"`).
4. **Fix Fragmented User Tables:** Instead of Raneem's disconnected `IAM_Authentication_Phone`, `IAM_Authentication_Email`, and `IAM_Authentication_Password` tables, add phone and verification properties directly onto our `Identity.Domain.Entities.User` aggregate.

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Extend `Identity.Domain` Entities & Errors
1. Update `src/Modules/Identity/Identity.Domain/Entities/User.cs`:
   - Add properties:
     - `public string? CountryCode { get; private set; }`
     - `public string? PhoneNumber { get; private set; }`
     - `public bool IsEmailVerified { get; private set; }`
     - `public bool IsPhoneVerified { get; private set; }`
   - Update `User.Create(...)` to accept optional `string? countryCode = null, string? phoneNumber = null, bool isEmailVerified = false`.
   - Add domain methods:
     - `public void SetPhone(string? countryCode, string? phoneNumber)`
     - `public void VerifyEmail()` -> sets `IsEmailVerified = true; UpdatedAtUtc = DateTime.UtcNow;`
     - `public void VerifyPhone()` -> sets `IsPhoneVerified = true; UpdatedAtUtc = DateTime.UtcNow;`
     - `public void UpdatePasswordHash(string newPasswordHash)` -> sets `PasswordHash = newPasswordHash; UpdatedAtUtc = DateTime.UtcNow;`
     - `public void UpdateContact(string email, string firstName, string lastName, string? countryCode, string? phoneNumber)`
2. Create `src/Modules/Identity/Identity.Domain/Enums/OtpPurpose.cs`:
   - `public enum OtpPurpose : byte { Register = 1, MobileLogin = 2, PasswordReset = 3 }`
3. Create `src/Modules/Identity/Identity.Domain/Entities/OtpVerification.cs` (`sealed class : Entity`):
   - Properties: `UserId` (Guid), `CountryCode` (string?), `PhoneNumber` (string?), `Email` (string?), `CodeHash` (string), `Purpose` (OtpPurpose), `ExpiresAtUtc` (DateTime), `CreatedAtUtc` (DateTime), `UsedAtUtc` (DateTime?), `FailedAttempts` (int).
   - Computed: `public bool IsValid => UsedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc && FailedAttempts < 5;`
   - Methods: `Create(...)`, `MarkAsUsed()`, `IncrementFailedAttempts()`.
4. Create `src/Modules/Identity/Identity.Domain/Entities/PasswordResetToken.cs` (`sealed class : Entity`):
   - Properties: `UserId` (Guid), `TokenHash` (string), `IsInviteToken` (bool), `CreatedAtUtc` (DateTime), `ExpiresAtUtc` (DateTime), `UsedAtUtc` (DateTime?).
   - Computed: `public bool IsValid => UsedAtUtc == null && DateTime.UtcNow < ExpiresAtUtc;`
   - Methods: `Create(...)`, `MarkAsUsed()`.
5. Create `src/Modules/Identity/Identity.Domain/Entities/LoginSession.cs` (`sealed class : Entity`) (ported from Raneem's `LoginSession.cs`):
   - Properties: `UserId` (Guid), `RefreshTokenId` (Guid?), `DeviceName` (string?), `DeviceType` (string?), `IpAddress` (string?), `UserAgent` (string?), `LoginAtUtc` (DateTime), `LogoutAtUtc` (DateTime?), `IsActive` (bool).
   - Methods: `Create(...)`, `TerminateSession()`.
6. Update `IdentityErrors.cs` with:
   - `PhoneAlreadyInUse` (Conflict 409), `PhoneNotFound` (NotFound 404), `InvalidOrExpiredOtp` (Validation 400), `InvalidOrExpiredResetToken` (Validation 400), `NotificationDeliveryFailed` (Failure 500).

#### Step 2: Add Notification & Hashing Ports in `Identity.Application`
1. `Abstractions/Ports/IWhatsAppNotificationPort.cs`:
   - `Task<Result> SendOtpMessageAsync(string countryCode, string phoneNumber, string otpCode, CancellationToken ct = default);`
2. `Abstractions/Ports/IEmailNotificationPort.cs`:
   - `Task<Result> SendPasswordResetEmailAsync(string toEmail, string fullName, string resetLink, CancellationToken ct = default);`
   - `Task<Result> SendEmailOtpAsync(string toEmail, string fullName, string otpCode, CancellationToken ct = default);`
3. `Abstractions/Ports/IOtpCodeGeneratorPort.cs`:
   - `string GenerateSixDigitCode();`
   - `string GenerateSecureUrlToken();`
   - `string ComputeSha256Hash(string rawValue);`
4. Update `IUserRepository.cs` to add methods for:
   - `GetByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default)`
   - `ExistsByPhoneAsync(string countryCode, string phoneNumber, CancellationToken ct = default)`
   - `AddOtpVerificationAsync(OtpVerification otp, CancellationToken ct = default)`
   - `GetLatestActiveOtpAsync(Guid userId, OtpPurpose purpose, CancellationToken ct = default)`
   - `AddPasswordResetTokenAsync(PasswordResetToken token, CancellationToken ct = default)`
   - `GetActivePasswordResetTokenByHashAsync(string tokenHash, CancellationToken ct = default)`
   - `AddLoginSessionAsync(LoginSession session, CancellationToken ct = default)`
   - `TerminateLoginSessionByRefreshTokenAsync(Guid refreshTokenId, CancellationToken ct = default)`

#### Step 3: Implement Raneem's Use Cases in `Identity.Application/Features/`
1. Update `RegisterUserCommand`:
   - Add optional `string? CountryCode = null, string? PhoneNumber = null` to `RegisterUserCommand`.
   - If `PhoneNumber` is provided, check `ExistsByPhoneAsync`, save `CountryCode` and `PhoneNumber` on `User`, generate a 6-digit OTP via `IOtpCodeGeneratorPort`, store its SHA-256 hash in `OtpVerification` (`OtpPurpose.Register`), and dispatch via `IWhatsAppNotificationPort.SendOtpMessageAsync`.
   - Ensure `RegisterUserResponse` NEVER contains `OtpCode`!
2. Create `Features/LoginMobile/LoginMobileCommand.cs`, `LoginMobileValidator.cs`, `LoginMobileCommandHandler.cs` (ported from Raneem's `LoginMobileCommand.cs`):
   - Takes `string CountryCode, string PhoneNumber`.
   - Looks up user via `GetByPhoneAsync`. Checks `user.Status == UserStatus.Active`.
   - Generates 6-digit OTP, stores `OtpVerification` (`OtpPurpose.MobileLogin`, 5-min expiry), sends via `IWhatsAppNotificationPort`, returns `Result.Success(new OtpDispatchedResponse("Verification code sent via WhatsApp.", 300))`.
3. Create `Features/VerifyOtp/VerifyOtpCommand.cs`, `VerifyOtpValidator.cs`, `VerifyOtpCommandHandler.cs` (ported from Raneem's `VerifyOtpCommand.cs`):
   - Takes `string? Email, string? CountryCode, string? PhoneNumber, string Code, string Purpose` (`"Register"` or `"MobileLogin"`).
   - Resolves user by Email or Phone. Finds latest active `OtpVerification`.
   - Compares `ComputeSha256Hash(request.Code)` with `otp.CodeHash`. If mismatch, calls `otp.IncrementFailedAttempts()`, saves, and returns `IdentityErrors.InvalidOrExpiredOtp`.
   - Marks `otp.MarkAsUsed()`, marks `user.VerifyPhone()` (or `VerifyEmail()`), generates JWT + RefreshToken + `LoginSession`, saves, and returns `AuthTokenResponse`.
4. Create `Features/ResendOtp/ResendOtpCommand.cs`, `ResendOtpValidator.cs`, `ResendOtpCommandHandler.cs` (ported from Raneem's `ResendOtpCommand.cs`):
   - Regenerates a new 6-digit OTP, saves `OtpVerification`, and sends via WhatsApp/Email.
5. Create `Features/ForgotPassword/ForgotPasswordCommand.cs`, `ForgotPasswordValidator.cs`, `ForgotPasswordCommandHandler.cs` (ported from Raneem's `ForgotPasswordCommand.cs`):
   - Takes `string Email`. Finds user by email (if not found, return generic 200 success message to prevent email enumeration!).
   - Generates cryptographic token via `GenerateSecureUrlToken()`, hashes with SHA-256, saves `PasswordResetToken` (15-min expiry, `IsInviteToken = false`), builds reset URL `{FrontendResetUrl}?token={Uri.EscapeDataString(rawToken)}`, and sends via `IEmailNotificationPort.SendPasswordResetEmailAsync`.
6. Create `Features/ResetPassword/ResetPasswordCommand.cs`, `ResetPasswordValidator.cs`, `ResetPasswordCommandHandler.cs` (ported from Raneem's `ChangePasswordCommand.cs`):
   - Takes `string Token, string NewPassword` (validated with strong password rules).
   - Computes SHA-256 hash of `request.Token`, queries `GetActivePasswordResetTokenByHashAsync`.
   - If null or `!token.IsValid`, returns `IdentityErrors.InvalidOrExpiredResetToken`.
   - Loads `User`, hashes `NewPassword` via `IPasswordHasherPort`, calls `user.UpdatePasswordHash(newHash)`, marks `token.MarkAsUsed()`, and calls `RevokeAllActiveUserTokensAsync(user.Id, "PasswordReset")` for security.

#### Step 4: Implement Raneem's Driven Adapters in `Identity.Infrastructure`
1. Create `Security/OtpCodeGeneratorAdapter.cs` implementing `IOtpCodeGeneratorPort` using `RandomNumberGenerator.GetInt32(100000, 1000000)` and `SHA256.HashData`.
2. Create `Notifications/UltraMsgOptions.cs` (`InstanceId`, `Token`, `BaseUrl = "[https://api.ultramsg.com](https://api.ultramsg.com)"`) and `Notifications/UltraMsgWhatsAppAdapter.cs` implementing `IWhatsAppNotificationPort`:
   - Port Raneem's exact HTTP POST call from `Raneem/Auth/dotnet/infrastructure/UltraMsgWhatsAppService.cs` (`FormUrlEncodedContent` with `token`, `to = $"{countryCode}{phoneNumber}"`, `body = $"Your verification code is: {otpCode}"` to `{BaseUrl}/{InstanceId}/messages/chat`).
   - If `InstanceId` or `Token` is not configured in `appsettings.json`, log a warning via `ILogger<UltraMsgWhatsAppAdapter>` without throwing an unhandled exception so local testing works smoothly without live UltraMsg credentials.
3. Create `Notifications/SmtpOptions.cs` (`Host`, `Port`, `Username`, `Password`, `FromEmail`, `FromName`, `FrontendResetUrl`) and `Notifications/SmtpEmailAdapter.cs` implementing `IEmailNotificationPort`:
   - Port Raneem's `SmtpClient` logic from `Raneem/Auth/dotnet/infrastructure/SmtpEmailService.cs`. If `Host` or `Username` is empty in local dev, log a warning via `ILogger` instead of crashing.
4. Create EF Core Configurations in `Identity.Infrastructure/Persistence/Configurations/`:
   - Update `UserConfiguration.cs` for `CountryCode` (max 10), `PhoneNumber` (max 25), `IsEmailVerified`, `IsPhoneVerified`, and filtered index on `(CountryCode, PhoneNumber)`.
   - Create `OtpVerificationConfiguration.cs` (`[identity].[OtpVerifications]`).
   - Create `PasswordResetTokenConfiguration.cs` (`[identity].[PasswordResetTokens]`).
   - Create `LoginSessionConfiguration.cs` (`[identity].[LoginSessions]`).
   - Add `DbSet` properties to `IdentityDbContext` and update `UserRepository.cs` and `DependencyInjection.cs`.

#### Step 5: Expose Endpoints in `Identity.Presentation/Controllers/AuthController.cs`
Add the following endpoints to `AuthController.cs`:
- `POST /api/v1/auth/login/mobile` (`[AllowAnonymous]`)
- `POST /api/v1/auth/verify-otp` (`[AllowAnonymous]`)
- `POST /api/v1/auth/resend-otp` (`[AllowAnonymous]`)
- `POST /api/v1/auth/forgot-password` (`[AllowAnonymous]`)
- `POST /api/v1/auth/reset-password` (`[AllowAnonymous]`)

Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```

---

## 🔵 PROMPT U4: Execute Phase U4 (Port Marwan's 5 OAuth2 Social Login Providers & Fix Security Flaws)

```text
You are executing Phase U4 of our Team Unification Plan (Porting Marwan's 5 OAuth2 Social Login Providers).

### MANDATORY PRE-READING
Read Marwan's actual source files in `Marwan/Authentication/dotnet/`:
1. `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/GoogleAuthService.cs`
2. `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/FacebookAuthService.cs`
3. `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/GitHubAuthService.cs`
4. `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/LinkedInAuthService.cs`
5. `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/MicrosoftAuthService .cs`
6. `Marwan/Authentication/dotnet/Authentication.Application/Services/AuthenticationService.cs` (`SocialLoginAsync` method)

### CRITICAL FLAWS IN MARWAN'S CODE YOU MUST FIX
1. **Decouple from `Microsoft.AspNetCore.Identity`:** Marwan used `UserManager<ApplicationUser>` and `AspNetUserLogins`. You MUST NOT use `Microsoft.AspNetCore.Identity`. Instead, create a pure domain entity `ExternalLogin : Entity` mapped to `[identity].[ExternalLogins]`.
2. **Fix Empty Email Account Collision & Crash:** In `FacebookAuthService.cs` and `GitHubAuthService.cs`, if the user hid their email, Marwan returned `Email = string.Empty` and then called `FindByEmailAsync("")` and created a user with `Email = ""`! You MUST validate that `!string.IsNullOrWhiteSpace(externalUser.Email)` and return `Result.Failure(IdentityErrors.SocialEmailRequired)` if the provider fails to return a valid email.
3. **Fix Account Pre-Hijacking Vulnerability:** When a social login matches an existing local user by email whose email was NOT previously verified (`!user.IsEmailVerified`), Marwan linked the social account but left the attacker's unverified local password active! You MUST invalidate/randomize the existing local `PasswordHash` if `!user.IsEmailVerified` when linking a verified social provider for the first time, then call `user.VerifyEmail()`.
4. **Fix Unhandled Configuration Exceptions:** Marwan threw `new InvalidOperationException("Google ClientId is not configured")` which caused HTTP 500 crashes. Instead, return `Result.Failure<ExternalUserInfo>(IdentityErrors.SocialProviderNotConfigured(ProviderName))`.
5. **Add Missing FluentValidation:** Marwan had no FluentValidation on `SocialLoginRequest`. Create `SocialLoginValidator : AbstractValidator<SocialLoginCommand>` validating `Provider` and `Credential`.

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Install NuGet Package
In `src/Modules/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj`, install `Google.Apis.Auth` version `1.68.0`.

#### Step 2: Update `Identity.Domain`
1. Create `src/Modules/Identity/Identity.Domain/Entities/ExternalLogin.cs` (`sealed class : Entity`):
   - Properties: `UserId` (Guid), `Provider` (string, e.g. `"Google"`), `ProviderKey` (string, external provider's user ID), `Email` (string), `LinkedAtUtc` (DateTime).
   - Factory: `public static ExternalLogin Create(Guid userId, string provider, string providerKey, string email)`.
2. In `IdentityErrors.cs`, add:
   - `UnsupportedSocialProvider(string provider)` (Validation 400)
   - `SocialProviderNotConfigured(string provider)` (Failure 500)
   - `InvalidSocialCredential(string provider)` (Unauthorized 401)
   - `SocialEmailRequired(string provider)` (Validation 400)

#### Step 3: Add Port, DTO & CQRS Handler in `Identity.Application`
1. Create `DTOs/ExternalUserInfo.cs`:
   ```csharp
   namespace Identity.Application.DTOs;
   public sealed record ExternalUserInfo(
       string Provider,
       string ProviderUserId,
       string Email,
       string FirstName,
       string LastName,
       bool EmailVerified);
   ```
2. Create `Abstractions/Ports/ISocialAuthProviderPort.cs`:
   ```csharp
   using Identity.Application.DTOs;
   using SharedKernel.Application;
   namespace Identity.Application.Abstractions.Ports;

   public interface ISocialAuthProviderPort
   {
       string ProviderName { get; }
       Task<Result<ExternalUserInfo>> ValidateAsync(string credential, CancellationToken ct = default);
   }
   ```
3. Update `IUserRepository.cs`:
   - `Task<User?> GetByExternalLoginAsync(string provider, string providerKey, CancellationToken ct = default);`
   - `Task<bool> ExternalLoginExistsAsync(Guid userId, string provider, CancellationToken ct = default);`
   - `Task AddExternalLoginAsync(ExternalLogin externalLogin, CancellationToken ct = default);`
4. Create `Features/SocialLogin/SocialLoginCommand.cs`:
   - `public sealed record SocialLoginCommand(string Provider, string Credential) : ICommand<AuthTokenResponse>;`
5. Create `Features/SocialLogin/SocialLoginValidator.cs`:
   - Validates `Provider` is not empty and is one of `["google", "facebook", "github", "linkedin", "microsoft"]` (case-insensitive).
   - Validates `Credential` is not empty.
6. Create `Features/SocialLogin/SocialLoginCommandHandler.cs`:
   - Inject `IEnumerable<ISocialAuthProviderPort> _socialProviders`, `IUserRepository`, `IPasswordHasherPort`, `IRbacAccessPort`, `IUserProfilePort`, `IJwtProviderPort`.
   - Find provider matching `request.Provider` (case-insensitive). If null, return `IdentityErrors.UnsupportedSocialProvider`.
   - Call `await provider.ValidateAsync(request.Credential, cancellationToken)`. If failure, return error.
   - Check `string.IsNullOrWhiteSpace(extUser.Email) || !extUser.EmailVerified` -> return `IdentityErrors.SocialEmailRequired`.
   - Look up user via `GetByExternalLoginAsync(extUser.Provider, extUser.ProviderUserId)`.
   - If null, look up via `GetByEmailAsync(extUser.Email.Trim().ToLowerInvariant())`:
     - If user exists by email:
       - **Security Fix (Pre-Hijacking Prevention):** If `!user.IsEmailVerified`, overwrite `user.UpdatePasswordHash(_passwordHasher.HashPassword(Guid.NewGuid().ToString("N")))` so any unverified pre-registered attacker password is invalidated, then call `user.VerifyEmail()`.
       - Add `ExternalLogin.Create(user.Id, extUser.Provider, extUser.ProviderUserId, extUser.Email)`.
     - If user does NOT exist by email:
       - Create `User.Create(extUser.Email, _passwordHasher.HashPassword(Guid.NewGuid().ToString("N")), extUser.FirstName, extUser.LastName, isEmailVerified: true)`.
       - Add user and `ExternalLogin`, save changes, call `_rbacAccessPort.AssignDefaultRoleAsync(user.Id, "StandardUser")`, and call `_userProfilePort.CreateOrSyncProfileAsync(...)`.
   - Verify `user.Status == UserStatus.Active`.
   - Fetch roles/permissions from `_rbacAccessPort.GetUserAccessProfileAsync(user.Id)`, generate token pair via `_jwtProvider.GenerateTokenPair(...)`, save `RefreshToken`, and return `Result.Success(authTokenResponse)`.

#### Step 4: Port Marwan's 5 Social Providers into `Identity.Infrastructure/Social/`
Port the exact token validation and HTTP exchange logic from Marwan's 5 files into `Identity.Infrastructure/Social/`:
1. `SocialAuthOptions.cs`: Configuration POCO binding section `"Authentication"` with sub-objects for `Google` (`ClientId`), `Facebook` (`AppId`, `AppSecret`), `GitHub` (`ClientId`, `ClientSecret`, `RedirectUri`), `LinkedIn` (`ClientId`, `ClientSecret`, `RedirectUri`), and `Microsoft` (`ClientId`, `ClientSecret`, `RedirectUri`, `TenantId`).
2. `GoogleAuthAdapter.cs` (`ProviderName = "Google"`): Uses `GoogleJsonWebSignature.ValidateAsync(credential, new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { clientId } })`.
3. `FacebookAuthAdapter.cs` (`ProviderName = "Facebook"`): Calls Facebook Graph API `debug_token` and `me?fields=id,name,email` just like Marwan's `FacebookAuthService.cs`.
4. `GitHubAuthAdapter.cs` (`ProviderName = "GitHub"`): Exchanges code at `[https://github.com/login/oauth/access_token](https://github.com/login/oauth/access_token)`, fetches user from `[https://api.github.com/user](https://api.github.com/user)` and primary verified email from `[https://api.github.com/user/emails](https://api.github.com/user/emails)` just like Marwan's `GitHubAuthService.cs`.
5. `LinkedInAuthAdapter.cs` (`ProviderName = "LinkedIn"`): Exchanges code at `[https://www.linkedin.com/oauth/v2/accessToken](https://www.linkedin.com/oauth/v2/accessToken)` and fetches OpenID userinfo from `[https://api.linkedin.com/v2/userinfo](https://api.linkedin.com/v2/userinfo)` just like Marwan's `LinkedInAuthService.cs`.
6. `MicrosoftAuthAdapter.cs` (`ProviderName = "Microsoft"`): Exchanges code at `[https://login.microsoftonline.com/](https://login.microsoftonline.com/){tenant}/oauth2/v2.0/token` and fetches profile from `[https://graph.microsoft.com/v1.0/me](https://graph.microsoft.com/v1.0/me)` just like Marwan's `MicrosoftAuthService .cs`.
7. Create `Persistence/Configurations/ExternalLoginConfiguration.cs` mapping `[identity].[ExternalLogins]` with unique index `UX_ExternalLogins_Provider_Key` on `(Provider, ProviderKey)`.
8. Register `AddHttpClient()` and all 5 `ISocialAuthProviderPort` implementations in `Identity.Infrastructure/DependencyInjection.cs`.

#### Step 5: Expose Endpoint in `Identity.Presentation/Controllers/AuthController.cs`
- Add `[HttpPost("social-login")]` (`[AllowAnonymous]`) dispatching `SocialLoginCommand`.
- Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```

---

## 🔵 PROMPT U5: Execute Phase U5 (Port Marwan's HttpOnly Cookies & ASP.NET Core Rate Limiting)

```text
You are executing Phase U5 of our Team Unification Plan (Porting Marwan's Cookie Service & Rate Limiting).

### MANDATORY PRE-READING
Read Marwan's actual files:
1. `Marwan/Authentication/dotnet/Authentication.Presentation/Services/AuthenticationCookieService.cs`
2. `Marwan/Authentication/dotnet/Authentication.Presentation/Program.cs` (Rate Limiter policies `AuthSensitive`, `AuthEmail`, `AuthLogin`)
3. `Marwan/Authentication/dotnet/Authentication.Presentation/Controllers/AuthenticationController.cs`

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Port `AuthCookieService` into `Identity.Presentation`
1. Create `src/Modules/Identity/Identity.Presentation/Services/IAuthCookieService.cs` and `AuthCookieService.cs`:
   - Port Marwan's cookie logic from `AuthenticationCookieService.cs`:
     - `SetTokenCookies(HttpResponse response, string accessToken, DateTime accessTokenExpiresAtUtc, string refreshToken, DateTime refreshTokenExpiresAtUtc)`:
       - Appends `"accessToken"` cookie (`HttpOnly = true`, `Secure = true`, `SameSite = SameSiteMode.Strict`, `Expires = accessTokenExpiresAtUtc`, `Path = "/"`).
       - Appends `"refreshToken"` cookie (`HttpOnly = true`, `Secure = true`, `SameSite = SameSiteMode.Strict`, `Expires = refreshTokenExpiresAtUtc`, `Path = "/"`).
     - `ClearTokenCookies(HttpResponse response)`: Deletes `"accessToken"` and `"refreshToken"` cookies.
2. Update `Identity.Presentation/Controllers/AuthController.cs`:
   - Inject `IAuthCookieService` (or invoke it on `HttpContext.Response`).
   - On successful `login`, `verify-otp`, `refresh`, and `social-login`, call `SetTokenCookies(...)` AND still return the full `AuthTokenResponse` in the HTTP JSON body (so both browser cookie clients and Swagger Bearer token testers work seamlessly!).
   - In `refresh` and `revoke` endpoints, make the request body optional (`[FromBody] RefreshTokenCommand? request = null`): if `request?.RefreshToken` is null or empty, read `Request.Cookies["refreshToken"]`! On `revoke`, call `ClearTokenCookies(Response)`.

#### Step 2: Dual Token Extraction in JWT Bearer Middleware (`AuthService.API`)
In `src/Host/AuthService.API/Program.cs`, inside `.AddJwtBearer(options => ...)`, add a `JwtBearerEvents` handler so the API accepts JWTs from EITHER the `Authorization: Bearer <token>` header (Swagger) OR the `"accessToken"` HttpOnly cookie (Marwan's browser flow):
```csharp
options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        if (string.IsNullOrEmpty(context.Token) &&
            context.Request.Cookies.TryGetValue("accessToken", out var cookieToken))
        {
            context.Token = cookieToken;
        }
        return Task.CompletedTask;
    }
};
```

#### Step 3: Port Marwan's Rate Limiting Policies into `AuthService.API`
1. Create `src/Host/AuthService.API/Extensions/RateLimitingExtensions.cs`:
   - Port Marwan's exact 3 FixedWindow/SlidingWindow rate limiter policies from `Marwan/Authentication/dotnet/Authentication.Presentation/Program.cs`:
     - `"AuthLogin"`: PermitLimit = 5, Window = 1 minute, QueueLimit = 0 (partitioned by remote IP address).
     - `"AuthEmail"`: PermitLimit = 3, Window = 1 minute, QueueLimit = 0 (partitioned by remote IP address).
     - `"AuthSensitive"`: PermitLimit = 5, Window = 5 minutes, QueueLimit = 0 (partitioned by remote IP address).
   - Configure `OnRejected` to return HTTP `429 Too Many Requests` with a standardized RFC 7807 `ProblemDetails` JSON body.
2. In `Identity.Presentation/Controllers/AuthController.cs`:
   - Add `[EnableRateLimiting("AuthLogin")]` to `login`, `login/mobile`, and `social-login`.
   - Add `[EnableRateLimiting("AuthEmail")]` to `register`, `resend-otp`, and `forgot-password`.
   - Add `[EnableRateLimiting("AuthSensitive")]` to `verify-otp` and `reset-password`.
3. Register `builder.Services.AddAuthRateLimiting()` and `app.UseRateLimiter()` in `AuthService.API/Program.cs`.
4. Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```

---

## 🔵 PROMPT U6: Execute Phase U6 (Regenerate All 3 Module Migrations, Unified Seeder & Config)

```text
You are executing Phase U6 (Final Database Migrations across `[identity]`, `[rbac]`, and `[users]` Schemas, Seeding & Readiness).

### STEP-BY-STEP EXECUTION INSTRUCTIONS

#### Step 1: Regenerate Clean EF Core Migrations for All 3 DbContexts
1. Delete existing files inside `src/Modules/Identity/Identity.Infrastructure/Persistence/Migrations/` (if any) and run:
   ```powershell
   dotnet ef migrations add InitialIdentitySchema --project "src/Modules/Identity/Identity.Infrastructure" --startup-project "src/Host/AuthService.API" --context IdentityDbContext --output-dir Persistence/Migrations
   ```
2. Ensure `AccessControlDbContext` migration exists in `src/Modules/AccessControl/AccessControl.Infrastructure/Persistence/Migrations/` (or generate `InitialRbacSchema` if not yet created).
3. Generate migration for `UsersDbContext`:
   ```powershell
   dotnet ef migrations add InitialUsersSchema --project "src/Modules/Users/Users.Infrastructure" --startup-project "src/Host/AuthService.API" --context UsersDbContext --output-dir Persistence/Migrations
   ```

#### Step 2: Update `DatabaseSeeder.cs` in `AuthService.API`
Update `src/Host/AuthService.API/Seeding/DatabaseSeeder.cs` to:
1. Call `await identityContext.Database.MigrateAsync()`, `await rbacContext.Database.MigrateAsync()`, and `await usersContext.Database.MigrateAsync()`.
2. Seed the 6 RBAC permissions (`auth:profile:read`, `users:read`, `users:manage`, `roles:read`, `roles:write`, `roles:manage`).
3. Seed the 3 system roles (`SuperAdmin`, `Manager`, `StandardUser`) and their role-permission links.
4. Seed the 2 default users in `IdentityDbContext` (`admin@authservice.local` / `Admin@12345!` with phone `+20` `1000000001` verified, and `user@authservice.local` / `User@12345!` with phone `+20` `1000000002` verified).
5. Seed the corresponding 2 `UserProfile` records in `UsersDbContext` (`[users].[UserProfiles]`) with matching `UserId` GUIDs so all 3 modules (`[identity]`, `[rbac]`, `[users]`) are 100% synchronized out of the box!

#### Step 3: Update `appsettings.json` & `README.md`
1. In `src/Host/AuthService.API/appsettings.json`, include complete configuration sections for:
   - `ConnectionStrings:DefaultConnection`
   - `JwtOptions`
   - `UltraMsg` (`InstanceId`, `Token`, `BaseUrl`)
   - `Smtp` (`Host`, `Port`, `Username`, `Password`, `FromEmail`, `FromName`, `FrontendResetUrl`)
   - `Authentication` (`Google`, `Facebook`, `GitHub`, `LinkedIn`, `Microsoft`)
2. Update root `README.md` documenting the 3 unified modules (`Identity`, `Users`, `AccessControl`), all REST endpoints across all controllers, and the pre-seeded credentials.
3. Run `dotnet build "D:\codes\Auth Service\AuthMicroservice.sln" --no-incremental` and verify 0 warnings and 0 errors.
```