# Team Codebase Unification & Modular Refactoring Master Plan

**Objective:** Unify the team's 4 separate implementations (`src/`, `Raneem/Auth`, `Marwan/Authentication`, `Raneem/users`) into our clean `.NET 8.0` Modular Hexagonal & Clean Architecture solution without losing any teammate's actual business logic or violating architectural boundaries.

---

## Global Unification Rules (STRICT CONSTRAINTS FOR AI AGENT)
1. **NO Re-Implementation from Scratch:** You MUST read the actual source files inside `Raneem/` and `Marwan/` before porting any feature. Preserve their actual API calls, HTTP payloads, cookie settings, rate-limiting rules, and provider logic, while adapting them to our `Result<T>`, `MediatR`, and `Ports & Adapters` patterns.
2. **NO Architectural Deviation:**
   - Do NOT import `Microsoft.AspNetCore.Identity` (`IdentityUser`) from Marwan's code. Keep our pure domain entities.
   - Do NOT throw raw `Exception("...")` like Raneem's code. Convert all failure branches into typed `Error` and `Result.Failure(...)`.
   - Do NOT expose `OtpCode` in any HTTP response DTO or `Console.WriteLine` (fixing Raneem's security leak).
   - Keep all projects strictly on **`net8.0`**.
3. **Presentation Layer Per Module:** Each module (`Identity`, `AccessControl`, `Users`) must own its own `.Presentation` Class Library containing its REST Controllers. `AuthService.API` acts strictly as the Host/Bootstrapper.

---

## Phase U1: Modular Presentation Refactoring (`Identity.Presentation` & `AccessControl.Presentation`)
**Goal:** Move Controllers out of `AuthService.API` into dedicated `.Presentation` projects inside each module (adopting Raneem & Marwan's modular presentation separation).
- Create `src/Modules/Identity/Identity.Presentation/Identity.Presentation.csproj` (`net8.0` with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`).
- Create `src/Modules/AccessControl/AccessControl.Presentation/AccessControl.Presentation.csproj` (`net8.0` with `<FrameworkReference Include="Microsoft.AspNetCore.App" />`).
- Move `AuthController.cs` to `Identity.Presentation/Controllers/`.
- Move `RbacController.cs` and custom Authorization attributes (`HasPermissionAttribute`) to `AccessControl.Presentation` (or `SharedKernel` / `Host` as appropriate so all modules can use `[HasPermission]`).
- Register `.AddApplicationPart(...)` in `AuthService.API/Program.cs` so Swagger discovers controllers across all module presentation assemblies.

---

## Phase U2: Scaffolding & Porting Module 3 — `Users` Module (`Raneem/users`)
**Goal:** Extract and refactor the third teammate's `Users` service (`Raneem/users/dotnet/`) into a clean 4-layer module (`Users.Domain`, `Users.Application`, `Users.Infrastructure`, `Users.Presentation`) using isolated schema `[users]` instead of hijacking `[identity]` tables.
- Create the 4 projects under `src/Modules/Users/`.
- Port user profile management (`CreateUser` with Admin Magic Link invite flow, `UpdateUser`, `GetUserById`, `GetUserByPhone`, `SoftDeleteUser`).
- Eliminate direct DB coupling to `IAM_Authentication_*` tables; use `SharedKernel.Contracts.IUserIdentityPort` for cross-module coordination with `Identity`.

---

## Phase U3: Porting Raneem's OTP, WhatsApp (`UltraMsg`), SMTP & Password Reset into `Identity`
**Goal:** Integrate Raneem's actual `UltraMsgWhatsAppService`, `SmtpEmailService`, Phone/Email OTP verification, `LoginSession` tracking, and `ForgotPassword` / `SetPassword` flows from `Raneem/Auth/dotnet/`.
- Read Raneem's actual files in `Raneem/Auth/dotnet/infrastructure/` and `application/`.
- Add `OtpRecord`, `PasswordResetToken`, and `LoginSession` entities to `Identity.Domain` (or extend `User` with `PhoneNumber`, `CountryCode`, `IsEmailVerified`, `IsPhoneVerified`).
- Create Outbound Ports in `Identity.Application`: `IWhatsAppNotificationPort`, `IEmailNotificationPort`.
- Implement Driven Adapters in `Identity.Infrastructure` using Raneem's exact `UltraMsgWhatsAppService` HTTP call logic and `SmtpEmailService` logic.
- Create CQRS Handlers & Endpoints for: `SendOtp`, `VerifyOtp`, `ResendOtp`, `LoginMobile`, `ForgotPassword`, and `ResetPassword` (ensuring OTP is hashed/stored securely and NEVER returned in the HTTP JSON response).

---

## Phase U4: Porting Marwan's OAuth2 Social Logins into `Identity`
**Goal:** Port Marwan's 5 Social Login providers (`Google`, `Facebook`, `GitHub`, `LinkedIn`, `Microsoft`) from `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/` into our Hexagonal Architecture while fixing his security bugs.
- Read Marwan's exact files in `Marwan/Authentication/dotnet/Authentication.Infrastructure/Social/`.
- Add `ExternalLogin` entity (`Provider`, `ProviderKey`, `UserId`) to `Identity.Domain` and map to `[identity].[ExternalLogins]`.
- Create Outbound Port `ISocialAuthProviderPort` in `Identity.Application`.
- Port Marwan's 5 provider classes into `Identity.Infrastructure/Social/` implementing the port.
- Implement `SocialLoginCommand`, `SocialLoginValidator` (with FluentValidation), and `SocialLoginCommandHandler` in `Identity.Application`, fixing:
  1. Null/empty email validation check (reject if provider returns empty email).
  2. Account pre-hijacking check (only auto-link if email is verified; assign default `StandardUser` role via `IRbacAccessPort` on new social user creation).

---

## Phase U5: Porting Marwan & Raneem's HttpOnly Cookies & Rate Limiting into `Host` / `Presentation`
**Goal:** Port Marwan's `AuthenticationCookieService` and `RateLimiting` policies (`AuthSensitive`, `AuthEmail`, `AuthLogin`) from `Marwan/Authentication/dotnet/Authentication.Presentation/`.
- Read Marwan's cookie service and `Program.cs` rate limiter setup.
- Add `IAuthCookieService` port/adapter so `login`, `refresh`, `social-login`, and `logout` both set `HttpOnly` secure cookies AND return JSON tokens (supporting both Swagger Bearer testing and browser cookie clients).
- Configure ASP.NET Core Rate Limiting middleware in `AuthService.API/Program.cs` and apply `[EnableRateLimiting("...")]` attributes to auth controllers.

---

## Phase U6: Unified EF Core Migrations (`[identity]`, `[rbac]`, `[users]`), Seeding & Full Verification
**Goal:** Update EF Core migrations across all 3 isolated module `DbContext` classes (`IdentityDbContext`, `AccessControlDbContext`, `UsersDbContext`), update `DatabaseSeeder`, and verify 0 errors.