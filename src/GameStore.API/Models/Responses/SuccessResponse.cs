namespace GameStore.API.Models.Responses;

/// <summary>
/// Resposta padronizada de sucesso retornada pela API.
/// </summary>
/// <param name="Message">Mensagem de sucesso traduzida para o idioma da requisição.</param>
public record SuccessResponse(string Message);

