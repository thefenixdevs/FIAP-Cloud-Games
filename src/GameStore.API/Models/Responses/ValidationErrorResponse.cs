namespace GameStore.API.Models.Responses;

/// <summary>
/// Resposta padronizada de erro de validação retornada pela API.
/// Contém erros organizados por campo para facilitar o tratamento no frontend.
/// </summary>
/// <param name="Message">Mensagem geral de erro de validação traduzida para o idioma da requisição.</param>
/// <param name="Errors">Dicionário com erros organizados por campo. A chave é o nome do campo e o valor é um array de mensagens de erro para aquele campo.</param>
public record ValidationErrorResponse(string Message, Dictionary<string, string[]> Errors);

