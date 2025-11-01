namespace GameStore.Domain.Common;

/// <summary>
/// Classe genérica para acumular violações de validação no domínio.
/// Organiza erros por campo (Dictionary) seguindo o mesmo formato usado na Application.
/// Segue DDD: estrutura de validação vive no núcleo do domínio.
/// </summary>
public sealed class ValidationErrors
{
    /// <summary>
    /// Dicionário de erros organizados por campo/chave.
    /// Cada chave representa um campo/propriedade, e o valor é um array de mensagens de erro (chaves de localização) para aquele campo.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Indica se não há violações (validação passou)
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Instância estática representando um resultado válido (sem erros)
    /// </summary>
    public static ValidationErrors Empty { get; } = new();

    /// <summary>
    /// Construtor privado padrão para criar instância vazia (sem erros)
    /// </summary>
    private ValidationErrors()
    {
        Errors = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Construtor privado que aceita dicionário de erros
    /// </summary>
    private ValidationErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        Errors = errors ?? new Dictionary<string, string[]>();
    }

    /// <summary>
    /// Cria uma nova instância adicionando uma violação para um campo específico
    /// </summary>
    /// <param name="field">Nome do campo/propriedade que falhou na validação</param>
    /// <param name="errorKey">Chave de localização da mensagem de erro</param>
    /// <returns>Nova instância de ValidationErrors com o erro adicionado</returns>
    public ValidationErrors AddError(string field, string errorKey)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(field));
        
        if (string.IsNullOrWhiteSpace(errorKey))
            throw new ArgumentException("Error key cannot be null or empty.", nameof(errorKey));

        var newErrors = new Dictionary<string, string[]>(Errors);
        
        if (newErrors.TryGetValue(field, out var existingErrors))
        {
            var errorList = new List<string>(existingErrors) { errorKey };
            newErrors[field] = errorList.ToArray();
        }
        else
        {
            newErrors[field] = new[] { errorKey };
        }

        return new ValidationErrors(newErrors);
    }

    /// <summary>
    /// Cria uma nova instância adicionando múltiplas violações para um campo específico
    /// </summary>
    /// <param name="field">Nome do campo/propriedade que falhou na validação</param>
    /// <param name="errorKeys">Chaves de localização das mensagens de erro</param>
    /// <returns>Nova instância de ValidationErrors com os erros adicionados</returns>
    public ValidationErrors AddErrors(string field, IEnumerable<string> errorKeys)
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("Field name cannot be null or empty.", nameof(field));
        
        if (errorKeys == null)
            throw new ArgumentNullException(nameof(errorKeys));

        var errorKeysList = errorKeys.Where(ek => !string.IsNullOrWhiteSpace(ek)).ToList();
        
        if (errorKeysList.Count == 0)
            return this;

        var newErrors = new Dictionary<string, string[]>(Errors);
        
        if (newErrors.TryGetValue(field, out var existingErrors))
        {
            var errorList = new List<string>(existingErrors);
            errorList.AddRange(errorKeysList);
            newErrors[field] = errorList.ToArray();
        }
        else
        {
            newErrors[field] = errorKeysList.ToArray();
        }

        return new ValidationErrors(newErrors);
    }

    /// <summary>
    /// Combina dois resultados de validação, mesclando os erros de ambos
    /// </summary>
    /// <param name="other">Outro ValidationErrors a ser mesclado</param>
    /// <returns>Nova instância de ValidationErrors com todos os erros combinados</returns>
    public ValidationErrors Merge(ValidationErrors? other)
    {
        if (other == null || other.IsValid)
            return this;

        if (IsValid)
            return other;

        var mergedErrors = new Dictionary<string, string[]>(Errors);

        foreach (var kvp in other.Errors)
        {
            if (mergedErrors.TryGetValue(kvp.Key, out var existingErrors))
            {
                var errorList = new List<string>(existingErrors);
                errorList.AddRange(kvp.Value);
                mergedErrors[kvp.Key] = errorList.Distinct().ToArray();
            }
            else
            {
                mergedErrors[kvp.Key] = kvp.Value;
            }
        }

        return new ValidationErrors(mergedErrors);
    }
}

