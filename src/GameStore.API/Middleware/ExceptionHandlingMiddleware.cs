using FluentValidation;
using GameStore.Domain.Exceptions;
using System.Net;
using System.Security;

namespace GameStore.API.Middleware;

/// <summary>
/// Middleware responsável por capturar e tratar exceções de forma centralizada
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationEx)
        {
            // Captura exceções de validação do FluentValidation
            _logger.LogWarning(validationEx, "Erro de validação detectado: {Message}", validationEx.Message);
            _logger.LogError("Erros de validação: {Errors}", validationEx.Errors);
            await HandleValidationExceptionAsync(context, validationEx);
            return;
        }
        catch (DomainRuleException domainEx)
        {
            // Captura exceções de regras de domínio
            _logger.LogWarning(domainEx, "Violação de regra de domínio detectada: {Message}", domainEx.Message);
            await HandleDomainRuleExceptionAsync(context, domainEx);
            return;
        }
        catch (BusinessRulesException businessEx)
        {
            // Captura exceções de regras de negócio
            _logger.LogWarning(businessEx, "Violação de regra de negócio detectada: {Message}", businessEx.Message);
            await HandleBusinessRulesExceptionAsync(context, businessEx);
            return;
        }
        catch (Exception ex)
        {   // Faz a captura de exceções não tratadas
            _logger.LogError(ex, "Um erro foi detectado: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
            return;
        }

        // Trata códigos de status de autenticação/autorização que não lançam exceção
        await HandleAuthenticationStatusCodesAsync(context);
    }

    /// <summary>
    /// Trata exceções de validação do FluentValidation
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    /// <param name="ex">A exceção de validação</param>
    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException ex)
    {
        var errorResponse = CreateValidationErrorResponse(ex);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)errorResponse.StatusCode;

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    /// <summary>
    /// Trata exceções de regras de domínio
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    /// <param name="ex">A exceção de regra de domínio</param>
    private static async Task HandleDomainRuleExceptionAsync(HttpContext context, DomainRuleException ex)
    {
        var errorResponse = CreateDomainRuleErrorResponse(ex);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)errorResponse.StatusCode;

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    /// <summary>
    /// Trata exceções de regras de negócio
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    /// <param name="ex">A exceção de regra de negócio</param>
    private static async Task HandleBusinessRulesExceptionAsync(HttpContext context, BusinessRulesException ex)
    {
        var errorResponse = CreateBusinessRulesErrorResponse(ex);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)errorResponse.StatusCode;

        await context.Response.WriteAsJsonAsync(errorResponse);
    }

    /// <summary>
    /// Trata exceções não tratadas
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    /// <param name="ex">A exceção</param>      
    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var errorResponse = CreateErrorResponse(ex); // Cria uma resposta de erro padrão
        context.Response.ContentType = "application/json"; // Define o tipo de conteúdo da resposta
        context.Response.StatusCode = (int)errorResponse.StatusCode; // Define o status code da resposta

        await context.Response.WriteAsJsonAsync(errorResponse); // Escreve a resposta no corpo da requisição
    }

    /// <summary>
    /// Trata códigos de status de autenticação/autorização que não lançam exceção
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    private static async Task HandleAuthenticationStatusCodesAsync(HttpContext context)
    {
        if (context.Response.HasStarted) return; // Se a resposta já foi iniciada, retorna

        switch (context.Response.StatusCode) // Verifica o status code da resposta
        {
            case (int)HttpStatusCode.Unauthorized:
                await WriteErrorResponseAsync(context, HttpStatusCode.Unauthorized, "Usuário não autenticado."); // Escreve uma resposta de erro para o usuário não autenticado
                break;
            case (int)HttpStatusCode.Forbidden:
                await WriteErrorResponseAsync(context, HttpStatusCode.Forbidden, "Usuário não autorizado."); // Escreve uma resposta de erro para o usuário não autorizado
                break;
        }
    }

    /// <summary>
    /// Cria uma resposta de erro padrão
    /// </summary>
    /// <param name="ex">A exceção</param>
    private static ErrorResponse CreateErrorResponse(Exception ex)
    {
        return ex switch // Verifica o tipo da exceção
        {
            UnauthorizedAccessException => new ErrorResponse(HttpStatusCode.Unauthorized, ex.Message), // Cria uma resposta de erro para o usuário não autenticado
            SecurityException => new ErrorResponse(HttpStatusCode.Forbidden, ex.Message),
            DomainRuleException domainEx => CreateDomainRuleErrorResponse(domainEx),
            BusinessRulesException businessEx => CreateBusinessRulesErrorResponse(businessEx),
            _ => new ErrorResponse(HttpStatusCode.InternalServerError, "Erro interno do servidor.") // Cria uma resposta de erro para erros internos do servidor
        };
    }

    /// <summary>
    /// Cria uma resposta de erro de validação
    /// </summary>
    /// <param name="ex">A exceção</param>
    private static ValidationErrorResponse CreateValidationErrorResponse(ValidationException ex)
    {
        var errors = ex.Errors // Obtém os erros da exceção
            .GroupBy(e => e.PropertyName) // Agrupa os erros por propriedade
            .ToDictionary(
                g => g.Key, // Obtém a chave da propriedade
                g => g.Select(e => e.ErrorMessage).ToArray() // Obtém as mensagens de erro
            );

        return new ValidationErrorResponse( // Cria uma resposta de erro de validação
            HttpStatusCode.BadRequest, // Define o status code da resposta
            "Erro de validação nos dados fornecidos.", // Define a mensagem da resposta
            errors // Define os erros da resposta
        );
    }

    /// <summary>
    /// Cria uma resposta de erro de regra de domínio
    /// </summary>
    /// <param name="ex">A exceção de regra de domínio</param>
    private static ValidationErrorResponse CreateDomainRuleErrorResponse(DomainRuleException ex)
    {
        if (ex.Errors.Any())
        {
            return new ValidationErrorResponse(
                HttpStatusCode.BadRequest,
                ex.Message,
                ex.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        return new ValidationErrorResponse(HttpStatusCode.BadRequest, ex.Message);
    }

    /// <summary>
    /// Cria uma resposta de erro de regra de negócio
    /// </summary>
    /// <param name="ex">A exceção de regra de negócio</param>
    private static ValidationErrorResponse CreateBusinessRulesErrorResponse(BusinessRulesException ex)
    {
        if (ex.Errors.Any())
        {
            return new ValidationErrorResponse(
                HttpStatusCode.BadRequest,
                ex.Message,
                ex.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            );
        }

        return new ValidationErrorResponse(HttpStatusCode.BadRequest, ex.Message);
    }

    /// <summary>
    /// Escreve uma resposta de erro no corpo da requisição
    /// </summary>
    /// <param name="context">O contexto da requisição</param>
    /// <param name="statusCode">O status code da resposta</param>
    /// <param name="message">A mensagem da resposta</param>
    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json"; // Define o tipo de conteúdo da resposta
        context.Response.StatusCode = (int)statusCode; // Define o status code da resposta

        var response = new ErrorResponse(statusCode, message); // Cria uma resposta de erro padrão
        await context.Response.WriteAsJsonAsync(response); // Escreve a resposta no corpo da requisição
    }
}

/// <summary>
/// Resposta de erro padrão
/// </summary>
public record ErrorResponse(HttpStatusCode StatusCode, string Message);

/// <summary>
/// Resposta de erro de validação com detalhes dos erros
/// </summary>
public record ValidationErrorResponse(
    HttpStatusCode StatusCode, 
    string Message, 
    Dictionary<string, string[]>? Errors = null
) : ErrorResponse(StatusCode, Message);