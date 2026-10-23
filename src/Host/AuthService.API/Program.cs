using System.Text;
using AccessControl.Application;
using AccessControl.Infrastructure;
using AccessControl.Infrastructure.Persistence;
using AuthService.API.Authorization;
using AuthService.API.Extensions;
using AuthService.API.Middleware;
using AuthService.API.Seeding;
using Identity.Application;
using Identity.Infrastructure;
using Identity.Infrastructure.Persistence;
using Identity.Infrastructure.Security;
using Identity.Presentation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Users.Application;
using Users.Infrastructure;
using Users.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON configuration
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Identity.Presentation.Controllers.AuthController).Assembly)
    .AddApplicationPart(typeof(AccessControl.Presentation.Controllers.RbacController).Assembly)
    .AddApplicationPart(typeof(Users.Presentation.Controllers.UsersController).Assembly);

// Swagger Documentation with JWT Bearer scheme
builder.Services.AddCustomSwagger();

// Global Exception Handler & ProblemDetails
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Modular Clean Architecture DI Registrations
builder.Services.AddIdentityApplication();
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddIdentityPresentation();
builder.Services.AddAccessControlApplication();
builder.Services.AddAccessControlInfrastructure(builder.Configuration);
builder.Services.AddUsersApplication();
builder.Services.AddUsersInfrastructure(builder.Configuration);

// Rate Limiting
builder.Services.AddAuthRateLimiting();

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>("IdentityDb")
    .AddDbContextCheck<AccessControlDbContext>("AccessControlDb")
    .AddDbContextCheck<UsersDbContext>("UsersDb");

// JWT Bearer Authentication (MapInboundClaims = false to preserve exact claim names)
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JwtOptions configuration section is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "roles",
        NameClaimType = "name"
    };

    // Dual token extraction: Header Bearer token OR HttpOnly accessToken cookie
    options.Events = new JwtBearerEvents
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
});

// Dynamic RBAC Permission Authorization
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth, Access Control & Users API v1");
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Apply migrations and seed data
await app.SeedDatabaseAsync();

app.Run();