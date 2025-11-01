namespace GameStore.Application.Common.Results;

/// <summary>
/// Representa o resultado padronizado de uma operação na camada de aplicação.
/// Segue o Result Pattern para tratamento consistente de sucesso e falhas.
/// </summary>
/// <typeparam name="T">Tipo dos dados retornados em caso de sucesso</typeparam>
public sealed record ApplicationResult<T>
{
    private ApplicationResult(bool isSuccess, string message, T? data, IReadOnlyCollection<string>? errors, IReadOnlyDictionary<string, string[]>? fieldErrors)
    {
        IsSuccess = isSuccess;
        IsFailure = !isSuccess;
        Message = message;
        Data = data;
        Errors = errors ?? Array.Empty<string>();
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Dados retornados em caso de sucesso
    /// </summary>
    public T? Data { get; }

    /// <summary>
    /// Lista de erros (especialmente útil para erros de validação)
    /// Mantida para compatibilidade retroativa.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; }

    /// <summary>
    /// Erros organizados por campo/propriedade (chave: nome do campo, valor: array de mensagens de erro).
    /// Facilita o tratamento no frontend permitindo mapear erros específicos por campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }

    /// <summary>
    /// Cria um resultado de sucesso com dados
    /// </summary>
    /// <param name="data">Dados a serem retornados</param>
    /// <param name="message">Mensagem de sucesso (opcional)</param>
    public static ApplicationResult<T> Success(T data, string message = "Operation completed successfully")
    {
        return new ApplicationResult<T>(true, message, data, null, null);
    }

    /// <summary>
    /// Cria um resultado de falha com mensagem
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public static ApplicationResult<T> Failure(string message)
    {
        return new ApplicationResult<T>(false, message, default, null, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com lista de erros
    /// </summary>
    /// <param name="errors">Lista de mensagens de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult<T> ValidationFailure(IReadOnlyCollection<string> errors, string message = "Validation failed")
    {
        return new ApplicationResult<T>(false, message, default, errors, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com um único erro
    /// </summary>
    /// <param name="error">Mensagem de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult<T> ValidationFailure(string error, string message = "Validation failed")
    {
        return new ApplicationResult<T>(false, message, default, new[] { error }, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com erros organizados por campo
    /// </summary>
    /// <param name="fieldErrors">Dicionário de erros organizados por campo (chave: nome do campo, valor: array de mensagens)</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult<T> ValidationFailure(IReadOnlyDictionary<string, string[]> fieldErrors, string message = "Validation failed")
    {
        var allErrors = fieldErrors?.Values.SelectMany(errors => errors).Distinct().ToList() ?? new List<string>();
        return new ApplicationResult<T>(false, message, default, allErrors, fieldErrors);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com um único erro para um campo específico
    /// </summary>
    /// <param name="fieldName">Nome do campo que falhou na validação</param>
    /// <param name="error">Mensagem de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult<T> ValidationFailure(string fieldName, string error, string message = "Validation failed")
    {
        var fieldErrors = new Dictionary<string, string[]> { { fieldName, new[] { error } } };
        return new ApplicationResult<T>(false, message, default, new[] { error }, fieldErrors);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com a mesma mensagem de erro aplicada a múltiplos campos.
    /// Útil para casos de segurança onde não se deve revelar qual campo específico está incorreto.
    /// </summary>
    /// <param name="fieldNames">Array com os nomes dos campos que devem receber o erro</param>
    /// <param name="errorMessage">Mensagem de erro de validação a ser aplicada a todos os campos</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult<T> ValidationFailure(string[] fieldNames, string errorMessage, string message = "Validation failed")
    {
        if (fieldNames == null || fieldNames.Length == 0)
        {
            return new ApplicationResult<T>(false, message, default, new[] { errorMessage }, null);
        }

        var fieldErrors = fieldNames.ToDictionary(fieldName => fieldName, _ => new[] { errorMessage });
        var allErrors = fieldErrors.Values.SelectMany(errors => errors).Distinct().ToList();
        return new ApplicationResult<T>(false, message, default, allErrors, fieldErrors);
    }
}

/// <summary>
/// Versão não genérica do ApplicationResult para operações que não retornam dados
/// </summary>
public sealed record ApplicationResult
{
    private ApplicationResult(bool isSuccess, string message, IReadOnlyCollection<string>? errors, IReadOnlyDictionary<string, string[]>? fieldErrors)
    {
        IsSuccess = isSuccess;
        IsFailure = !isSuccess;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
        FieldErrors = fieldErrors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indica se a operação falhou
    /// </summary>
    public bool IsFailure { get; }

    /// <summary>
    /// Mensagem descritiva do resultado
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Lista de erros (especialmente útil para erros de validação)
    /// Mantida para compatibilidade retroativa.
    /// </summary>
    public IReadOnlyCollection<string> Errors { get; }

    /// <summary>
    /// Erros organizados por campo/propriedade (chave: nome do campo, valor: array de mensagens de erro).
    /// Facilita o tratamento no frontend permitindo mapear erros específicos por campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> FieldErrors { get; }

    /// <summary>
    /// Cria um resultado de sucesso
    /// </summary>
    /// <param name="message">Mensagem de sucesso (opcional)</param>
    public static ApplicationResult Success(string message = "Operation completed successfully")
    {
        return new ApplicationResult(true, message, null, null);
    }

    /// <summary>
    /// Cria um resultado de falha com mensagem
    /// </summary>
    /// <param name="message">Mensagem de erro</param>
    public static ApplicationResult Failure(string message)
    {
        return new ApplicationResult(false, message, null, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com lista de erros
    /// </summary>
    /// <param name="errors">Lista de mensagens de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult ValidationFailure(IReadOnlyCollection<string> errors, string message = "Validation failed")
    {
        return new ApplicationResult(false, message, errors, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com um único erro
    /// </summary>
    /// <param name="error">Mensagem de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult ValidationFailure(string error, string message = "Validation failed")
    {
        return new ApplicationResult(false, message, new[] { error }, null);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com erros organizados por campo
    /// </summary>
    /// <param name="fieldErrors">Dicionário de erros organizados por campo (chave: nome do campo, valor: array de mensagens)</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult ValidationFailure(IReadOnlyDictionary<string, string[]> fieldErrors, string message = "Validation failed")
    {
        var allErrors = fieldErrors?.Values.SelectMany(errors => errors).Distinct().ToList() ?? new List<string>();
        return new ApplicationResult(false, message, allErrors, fieldErrors);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com um único erro para um campo específico
    /// </summary>
    /// <param name="fieldName">Nome do campo que falhou na validação</param>
    /// <param name="error">Mensagem de erro de validação</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult ValidationFailure(string fieldName, string error, string message = "Validation failed")
    {
        var fieldErrors = new Dictionary<string, string[]> { { fieldName, new[] { error } } };
        return new ApplicationResult(false, message, new[] { error }, fieldErrors);
    }

    /// <summary>
    /// Cria um resultado de falha de validação com a mesma mensagem de erro aplicada a múltiplos campos.
    /// Útil para casos de segurança onde não se deve revelar qual campo específico está incorreto.
    /// </summary>
    /// <param name="fieldNames">Array com os nomes dos campos que devem receber o erro</param>
    /// <param name="errorMessage">Mensagem de erro de validação a ser aplicada a todos os campos</param>
    /// <param name="message">Mensagem geral de erro (opcional)</param>
    public static ApplicationResult ValidationFailure(string[] fieldNames, string errorMessage, string message = "Validation failed")
    {
        if (fieldNames == null || fieldNames.Length == 0)
        {
            return new ApplicationResult(false, message, new[] { errorMessage }, null);
        }

        var fieldErrors = fieldNames.ToDictionary(fieldName => fieldName, _ => new[] { errorMessage });
        var allErrors = fieldErrors.Values.SelectMany(errors => errors).Distinct().ToList();
        return new ApplicationResult(false, message, allErrors, fieldErrors);
    }
}
