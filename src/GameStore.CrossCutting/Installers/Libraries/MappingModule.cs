using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MapsterMapper;

namespace GameStore.CrossCutting.Installers.Libraries;

public static class MappingModule
{
    public static IServiceCollection AddMapster(this IServiceCollection services)
    {
        // Carrega a assembly onde est�o os mapeamentos
        var assembly = Assembly.Load("GameStore.Application");

        // Configura o Mapster para escanear a assembly em busca de mapeamentos
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);

        // Registra o TypeAdapterConfig e o IMapper no cont�iner de DI
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}
