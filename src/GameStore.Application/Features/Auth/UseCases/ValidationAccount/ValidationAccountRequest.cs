namespace GameStore.Application.Features.Auth.UseCases.ValidationAccount;

/// <summary>
/// Request para validação de conta que aceita o código diretamente.
/// A decodificação será feita no handler, seguindo DDD.
/// </summary>
public record ValidationAccountRequest(string Code);

