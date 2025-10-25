using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameStore.CrossCutting.Localization
{
    public static class LocalizationDependencyInjection
    {
        public static IServiceCollection AddCustomLocalization(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LocalizationSettings>(configuration.GetSection("Localization"));
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddSingleton<ITranslationService, TranslationService>();
            return services;
        }
    }
}
