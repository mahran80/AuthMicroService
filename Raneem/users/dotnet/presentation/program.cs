using Users.Application;
using Users.Infrastructure;
using IAM.Services;
using Auth.Grpc;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

builder.Services.AddUsersApplication();
builder.Services.AddUsersInfrastructure(builder.Configuration);
builder.Services
    .AddGrpcClient<AuthService.AuthServiceClient>(options =>
    {
        options.Address = new Uri("https://localhost:7130");
    });
var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
    app.UseSwagger();
    app.UseSwaggerUI();
// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<UserGrpcService>();

app.Run();