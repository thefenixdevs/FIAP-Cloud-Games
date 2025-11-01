using GameStore.Domain.Common;

namespace GameStore.Application.Common.Exceptions;

/// <summary>
/// Exceção customizada para erros de validação na camada de aplicação.
/// Organiza erros por campo (Dictionary) para facilitar o tratamento no frontend.
/// Segue DDD: exceções de domínio/aplicação na camada apropriada.
/// </summary>
public sealed class ApplicationValidationException : Exception
{
    private static string MessageGeneric { get; } = "Ops! Detectamos uma violação em nosso sistema!";

    /// <summary>
    /// Dicionário de erros organizados por campo/chave.
    /// Cada chave representa um campo/propriedade, e o valor é um array de mensagens de erro para aquele campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Construtor padrão com mensagem genérica e sem erros específicos
    /// </summary>
    public ApplicationValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Construtor com uma única chave e mensagem de erro, usando mensagem genérica padrão
    /// </summary>
    /// <param name="key">Nome do campo/propriedade que falhou na validação</param>
    /// <param name="error">Mensagem de erro (chave de localização)</param>
    public ApplicationValidationException(string key, string error) : base(MessageGeneric)
    {
        Errors = new Dictionary<string, string[]> { { key, new[] { error } } };
    }

    /// <summary>
    /// Construtor com uma única chave, mensagem de erro e mensagem personalizada
    /// </summary>
    /// <param name="key">Nome do campo/propriedade que falhou na validação</param>
    /// <param name="error">Mensagem de erro (chave de localização)</param>
    /// <param name="message">Mensagem genérica personalizada para a exceção</param>
    public ApplicationValidationException(string key, string error, string message) : base(message)
    {
        Errors = new Dictionary<string, string[]> { { key, new[] { error } } };
    }

    /// <summary>
    /// Construtor com múltiplas chaves, uma única mensagem de erro aplicada a todas, e mensagem personalizada
    /// </summary>
    /// <param name="keys">Array de nomes de campos/propriedades que falharam na validação</param>
    /// <param name="error">Mensagem de erro (chave de localização) a ser aplicada a todos os campos</param>
    /// <param name="message">Mensagem genérica personalizada para a exceção</param>
    public ApplicationValidationException(string[] keys, string error, string message) : base(message)
    {
        Errors = keys.ToDictionary(key => key, key => new[] { error });
    }

    /// <summary>
    /// Construtor com múltiplas chaves e uma única mensagem de erro, usando mensagem genérica padrão
    /// </summary>
    /// <param name="keys">Array de nomes de campos/propriedades que falharam na validação</param>
    /// <param name="error">Mensagem de erro (chave de localização) a ser aplicada a todos os campos</param>
    public ApplicationValidationException(string[] keys, string error) : base(MessageGeneric)
    {
        Errors = keys.ToDictionary(key => key, key => new[] { error });
    }

    /// <summary>
    /// Construtor com dicionário completo de erros e mensagem personalizada
    /// </summary>
    /// <param name="errors">Dicionário completo de erros organizados por campo</param>
    /// <param name="message">Mensagem genérica personalizada para a exceção</param>
    public ApplicationValidationException(IReadOnlyDictionary<string, string[]> errors, string message) : base(message)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Construtor com dicionário completo de erros, usando mensagem genérica padrão
    /// </summary>
    /// <param name="errors">Dicionário completo de erros organizados por campo</param>
    public ApplicationValidationException(IReadOnlyDictionary<string, string[]> errors) : base(MessageGeneric)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Construtor que aceita ValidationErrors do Domain, usando mensagem genérica padrão
    /// </summary>
    /// <param name="validationErrors">ValidationErrors do Domain contendo as violações acumuladas</param>
    public ApplicationValidationException(ValidationErrors validationErrors) : base(MessageGeneric)
    {
        if (validationErrors == null)
            throw new ArgumentNullException(nameof(validationErrors));
        
        Errors = validationErrors.Errors;
    }

    /// <summary>
    /// Construtor que aceita ValidationErrors do Domain com mensagem personalizada
    /// </summary>
    /// <param name="validationErrors">ValidationErrors do Domain contendo as violações acumuladas</param>
    /// <param name="message">Mensagem genérica personalizada para a exceção</param>
    public ApplicationValidationException(ValidationErrors validationErrors, string message) : base(message)
    {
        if (validationErrors == null)
            throw new ArgumentNullException(nameof(validationErrors));
        
        Errors = validationErrors.Errors;
    }
}
