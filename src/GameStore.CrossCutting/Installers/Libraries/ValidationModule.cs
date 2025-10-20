using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.Installers.Libraries;

public static class ValidationModule
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        var assembly = Assembly.Load("GameStore.Application");
        services.AddValidatorsFromAssembly(assembly);
        
        return services;
    }
}
