using GameStore.API.Authorization;
using GameStore.API.Middleware;
using GameStore.Application;
using GameStore.CrossCutting.DependencyInjection;
using GameStore.Infrastructure;
using GameStore.Infrastructure.Data.Initialization;
using Microsoft.AspNetCore.Authorization;
using Serilog;


var builder = WebApplication.CreateBuilder(args);

// Configuração do Serilog
builder.AddSerilogLogging();

builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddApiSwagger();
// Configuração da autenticação JWT
builder.Services.AddApiAuthentication(builder.Configuration);

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("ConfirmedCommonUser", policy =>
      policy.Requirements.Add(new ConfirmedCommonUserRequirement()));
  options.AddPolicy("ConfirmedAdmin", policy =>
      policy.Requirements.Add(new ConfirmedAdminRequirement()));
});

builder.Services.AddSingleton<IAuthorizationHandler, ConfirmedCommonUserHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ConfirmedAdminHandler>();

// DI da camada Infrastructure (data, repositories, seeders)
builder.Services.AddInfrastructure(builder.Configuration);
// DI da camada Application (serviços, casos de uso, etc)
builder.Services.AddApplication();

var app = builder.Build();

// Inicialização do banco de dados: aplica migrations e executa seeders
await app.Services.InitializeDatabaseAsync();

if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI(options =>
  {
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Game Store API v1");
    options.RoutePrefix = string.Empty;
  });
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

Log.Information("Game Store API is starting...");

try
{
  app.Run();
}
catch (Exception ex)
{
  Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
  Log.CloseAndFlush();
}
