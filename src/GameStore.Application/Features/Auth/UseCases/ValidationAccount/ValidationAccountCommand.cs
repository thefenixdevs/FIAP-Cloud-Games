using GameStore.Application.Common.Results;
using Mediator;

namespace GameStore.Application.Features.Auth.UseCases.ValidationAccount;

/// <summary>
/// Command para validação de conta que aceita o código diretamente.
/// A decodificação do código será feita no handler, seguindo DDD.
/// </summary>
public sealed record ValidationAccountCommand(string Code) : IRequest<ApplicationResult>;

