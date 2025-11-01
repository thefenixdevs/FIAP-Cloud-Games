using GameStore.Domain.ValueObjects;
using Mapster;

namespace GameStore.Application.Common.Mappings;

/// <summary>
/// Configuração global de mappings e Value Objects
/// </summary>
public sealed class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Configurar mapeamento de Email Value Object para string
        config.NewConfig<Email, string>()
            .MapWith(email => email.Value);

        // Configurar mapeamento de string para Email (quando necessário)
        config.NewConfig<string, Email>()
            .MapWith(email => Email.Create(email));

        // Configurações globais
        config.Default
            .IgnoreNullValues(true);
    }
}

