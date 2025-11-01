using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace GameStore.CrossCutting.DependencyInjection;

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
        Description = "API RESTful para gerenciamento de uma loja de jogos com autenticação de usuários, seguindo arquitetura DDD (Domain-Driven Design). " +
                      "A API fornece endpoints para autenticação, gerenciamento de usuários e jogos, com sistema de autorização baseado em roles (Admin e Common User).",
        Contact = new OpenApiContact
        {
          Name = "Game Store API Support"
        }
      });

      // Incluir comentários XML da API
      var apiXmlFile = "GameStore.API.xml";
      var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
      
      if (File.Exists(apiXmlPath))
      {
        options.IncludeXmlComments(apiXmlPath);
      }
      
      // Incluir comentários XML do CrossCutting se existir
      var crossCuttingXmlFile = "GameStore.CrossCutting.xml";
      var crossCuttingXmlPath = Path.Combine(AppContext.BaseDirectory, crossCuttingXmlFile);
      
      if (File.Exists(crossCuttingXmlPath))
      {
        options.IncludeXmlComments(crossCuttingXmlPath);
      }

      // Configurar ordenação de operações (por tag e depois por método HTTP)
      options.OrderActionsBy(apiDesc => 
      {
        var tagOrder = apiDesc.GroupName ?? "Default";
        var methodOrder = apiDesc.HttpMethod switch
        {
          "GET" => "1",
          "POST" => "2",
          "PUT" => "3",
          "DELETE" => "4",
          "PATCH" => "5",
          _ => "9"
        };
        return $"{tagOrder}_{methodOrder}_{apiDesc.RelativePath}";
      });

      // Configurar tags com descrição
      options.TagActionsBy(api =>
      {
        return api.GroupName != null ? new[] { api.GroupName } : new[] { api.ActionDescriptor.RouteValues["controller"] ?? "Default" };
      });

      options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
      {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
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

      // Configurar para usar enum como string
      options.SchemaFilter<EnumSchemaFilter>();
    });

    return services;
  }
}

// Helper para exibir enums como strings no Swagger
internal class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
  public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
  {
    if (context.Type.IsEnum)
    {
      schema.Type = "string";
      schema.Enum.Clear();
      foreach (var enumValue in Enum.GetValues(context.Type))
      {
        schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumValue.ToString()));
      }
    }
  }
}
