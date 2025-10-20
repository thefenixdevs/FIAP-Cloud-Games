namespace GameStore.Application.Common.Options;

/// <summary>
/// Opções de configuração para a URL base da aplicação
/// </summary>
public sealed class BaseUrlOptions
{
    /// <summary>
    /// Seção de configuração no appsettings
    /// </summary>
    public const string SectionName = "BaseUrl";

    /// <summary>
    /// URL base da aplicação
    /// </summary>
    public string Url { get; set; } = string.Empty;
}
