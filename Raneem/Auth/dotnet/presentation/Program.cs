using Auth.Application;
using Auth.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Auth.Services; // أو الـ namespace اللي فيه AuthGrpcService

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
options.ListenAnyIP(7130, listenOptions =>
{
    listenOptions.UseHttps();
    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
});
    options.ListenAnyIP(5166, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1;
    });
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
  .WithOrigins(
    // "https://lady-driver-git-main-nada-w-abdelfattahs-projects.vercel.app",
    "http://localhost:4200",
    "https://lady-driver-three.vercel.app"
)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseCors("AngularPolicy");

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<AuthGrpcService>();

app.Run();