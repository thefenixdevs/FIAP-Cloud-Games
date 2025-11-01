using GameStore.Application.Features.Auth.UseCases.RegisterUser;
using Mapster;

namespace GameStore.Tests.TestUtils;

/// <summary>
/// Configuração do Mapster para os testes.
/// Garante que todos os mapeamentos IRegister sejam carregados, similar à configuração da aplicação.
/// </summary>
public static class MapsterTestSetup
{
    private static readonly object _lock = new();
    private static bool _initialized = false;

    /// <summary>
    /// Inicializa o Mapster com scan do assembly Application.
    /// Garante que seja chamado apenas uma vez, mesmo em execução paralela de testes.
    /// </summary>
    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        lock (_lock)
        {
            if (_initialized)
            {
                return;
            }

            var typeAdapterConfig = TypeAdapterConfig.GlobalSettings;
            typeAdapterConfig.Scan(typeof(RegisterUserCommand).Assembly);

            _initialized = true;
        }
    }
}

