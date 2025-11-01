namespace GameStore.API.Models.Responses;

/// <summary>
/// Resposta padronizada de erro retornada pela API.
/// </summary>
/// <param name="Message">Mensagem de erro descritiva traduzida para o idioma da requisição.</param>
public record ErrorResponse(string Message);

