using GameStore.Application.Common.Exceptions;
using GameStore.CrossCutting.Localization;
using System.Net;
using System.Security;
using System.Text.Json;

namespace GameStore.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly ITranslationService _translator;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, ITranslationService translator)
        {
            _next = next;
            _logger = logger;
            _translator = translator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex, _translator);
            }

            // Captura falhas de autenticação/autorização que não lançam exceção
            if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized && !context.Response.HasStarted)
            {
                await WriteJsonResponse(context, HttpStatusCode.Unauthorized, _translator.Translate("ExceptionHandlingMiddleware.UnauthenticatedUser"));
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden && !context.Response.HasStarted)
            {
                await WriteJsonResponse(context, HttpStatusCode.Forbidden, _translator.Translate("ExceptionHandlingMiddleware.UnauthorizedUser"));
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, ITranslationService translator)
        {
            context.Response.ContentType = "application/json";

            var statusCode = ex switch
            {
                ApplicationValidationException => HttpStatusCode.BadRequest,
                UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                SecurityException => HttpStatusCode.Forbidden,
                _ => HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = (int)statusCode;

            // Tratamento especial para ApplicationValidationException
            if (ex is ApplicationValidationException validationException)
            {
                var translatedMessage = translator.Translate(validationException.Message);
                
                // Formatar erros por campo traduzindo as mensagens (consistente com BaseController)
                var errors = validationException.Errors.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Select(e => translator.Translate(e)).ToArray()
                );

                // Incluir campo errors apenas se houver erros
                object response;
                if (errors.Count > 0)
                {
                    response = new
                    {
                        message = translatedMessage,
                        errors = errors
                    };
                }
                else
                {
                    response = new
                    {
                        message = translatedMessage
                    };
                }

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }

            // Para outras exceções, retornar mensagem padrão
            var defaultResponse = new
            {
                message = ex.Message,
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(defaultResponse));
        }

        private static async Task WriteJsonResponse(HttpContext context, HttpStatusCode statusCode, string message)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                message = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}