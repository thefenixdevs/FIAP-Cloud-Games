using Microsoft.OpenApi.Models;
using System.Reflection;

namespace GameStore.API.Extensions;

public static class SwaggerModule
{
  public static IServiceCollection AddApiSwagger(this IServiceCollection services)
  {
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(options =>
    {
      options.SwaggerDoc("v1", new OpenApiInfo
      {
        Title = "Game Store API",
        Version = "v1",
        Description = "A Web API for managing a game store with user authentication - DDD Architecture"
      });

      // Inclui comentários XML do assembly de entrada (API)
      var entryAssembly = Assembly.GetEntryAssembly();
      if (entryAssembly != null)
      {
        var apiXmlFile = $"{entryAssembly.GetName().Name}.xml";
        var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
        if (File.Exists(apiXmlPath))
        {
          options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
        }
      }

      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
      });

      options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    return services;
  }
}
