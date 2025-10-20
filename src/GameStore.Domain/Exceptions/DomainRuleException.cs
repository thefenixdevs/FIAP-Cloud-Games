namespace GameStore.Domain.Exceptions;

/// <summary>
/// Representa uma exceção lançada quando uma regra de domínio é violada.
/// </summary>
public class DomainRuleException : Exception
{
	public IReadOnlyDictionary<string, string[]> Errors { get; } = new Dictionary<string, string[]>();
	private static string MessageError { get; } = "Detectamos uma violação de nossas regras de negócio.";

	/// <summary>
	/// Inicializa uma nova instância da classe <see cref="DomainRuleException"/>.
	/// </summary>
	public DomainRuleException() : base(MessageError)
	{
	}

	/// <summary>
	///Inicializa uma nova instância da classe <see cref="DomainRuleException"/> 
	///com uma mensagem de erro personalizada.
	/// </summary>
	/// <param name="messsage">A mensagem personalizada de exibição </param>
	public DomainRuleException(string messsage) : base(messsage)
	{
	}

	/// <summary>
	///Inicializa uma nova instância da classe <see cref="DomainRuleException"/> 
	///com um conjunto de erros associados a uma chave específica.
	/// </summary>
	/// <param name="key">A chave que identifica a propriedade associada ao erro.</param>
	/// <param name="errors">Uma série de mensagens de erro relacionadas à chave.</param>
	public DomainRuleException(string key, string[] errors) : this(MessageError)
	{
		Errors = new Dictionary<string, string[]>
		{
			{ key, errors }
		};
	}

	/// <summary>
	/// Inicializa uma nova instância da classe <see cref="DomainRuleException"/>
	/// com um conjunto de erros associados a várias chaves.
	/// </summary>
	/// <param name="key">Uma matriz de chaves que identificam as propriedades de domínio associadas ao erro.</param>
	/// <param name="error">Messagem de erro relacionadas as chaves </param>
	public DomainRuleException(string[] key, string error) : this(MessageError)
	{
		var errorsDict = new Dictionary<string, string[]>();
		foreach (var k in key)
		{
			errorsDict.Add(k, [error]);
		}
		Errors = errorsDict;
	}

	/// <summary>
	/// Inicializa uma nova instância da classe <see cref="DomainRuleException"/>
	/// </summary>
	/// <param name="key">A chave que identifica a propriedade associada ao erro </param>
	/// <param name="error"> Mensagem de erro associada à chave </param>
	public DomainRuleException(string key, string error) : this(MessageError)
	{
		Errors = new Dictionary<string, string[]>
		{
			{ key, new[] { error } }
		};
	}

}
