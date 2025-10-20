namespace GameStore.Domain.Exceptions;

/// <summary>
/// Representa uma exceção que é lançada quando uma ou mais violações
/// de regras de negócios ocorrem na camada de aplicação.
/// </summary>
public class BusinessRulesException : Exception
{
	public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();
	private static string MessageGeneric { get; } = "Ops! Detectamos uma violação em nosso sistema!";

	public BusinessRulesException(string message) : base(message)
	{
	}

	public BusinessRulesException(string key, string message) : base(MessageGeneric)
	{
		Errors = new Dictionary<string, string[]> { { key, new[] { message } } };
	}

	public BusinessRulesException(string key, string error, string message) : base(message)
	{
		Errors = new Dictionary<string, string[]> { { key, new[] { error } } };
	}

	public BusinessRulesException(string[] keys, string error, string message) : base(message)
	{
		Errors = keys.ToDictionary(key => key, key => new[] { error });
	}
	public BusinessRulesException(string[] keys, string error) : base(MessageGeneric)
	{
		Errors = keys.ToDictionary(key => key, key => new[] { error });
	}

}
