using FluentValidation;
using GameStore.Application.Behaviors;
using Mapster;
using MapsterMapper;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using GameStore.Application.Features.Auth.UseCases.RegisterUser;

namespace GameStore.CrossCutting.DependencyInjection;

public static class ApplicationModule
{
  public static IServiceCollection AddApplication(this IServiceCollection services)
  {
    // Configurar Mapster com assembly scanning para mappings
    var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
    typeAdapterConfig.Scan(typeof(RegisterUserCommand).Assembly);
    
    services.AddSingleton(typeAdapterConfig);
    services.AddScoped<IMapper, ServiceMapper>();

    // Registrar validators do FluentValidation via assembly scanning
    services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);

    // Registrar Mediator com assembly scanning e pipeline behaviors
    services.AddMediator(options =>
    {
      options.Namespace = "GameStore.Application";
      options.ServiceLifetime = ServiceLifetime.Scoped;
      options.Assemblies = [typeof(RegisterUserCommand).Assembly];
      options.PipelineBehaviors = [typeof(ValidationBehavior<,>)];
    });

    // Nota: Implementações de serviços estão registradas em InfrastructureModule
    // Apenas as interfaces estão definidas em Application/Services

    return services;
  }
}
