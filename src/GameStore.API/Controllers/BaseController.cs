using System.Linq;
using GameStore.Application.Common.Results;
using GameStore.CrossCutting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace GameStore.API.Controllers;

/// <summary>
/// Controller base com métodos helper para padronizar respostas HTTP e tradução de mensagens.
/// Segue DDD: tradução acontece apenas na camada de apresentação.
/// </summary>
public abstract class BaseController : ControllerBase
{
    private readonly IStringLocalizer<SharedResource> _localizer;

    protected BaseController(IStringLocalizer<SharedResource> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Traduz uma chave de mensagem para o idioma atual da requisição
    /// </summary>
    protected string TranslatedMessage(string key)
    {
        return _localizer[key];
    }

    /// <summary>
    /// Formata erros para resposta padronizada.
    /// Se FieldErrors estiver disponível e populado, retorna dicionário por campo.
    /// Caso contrário, retorna array simples para compatibilidade retroativa.
    /// </summary>
    protected object FormatErrors(ApplicationResult result)
    {
        if (result.FieldErrors != null && result.FieldErrors.Count > 0)
        {
            // Formato padronizado: erros organizados por campo
            return result.FieldErrors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(e => TranslatedMessage(e)).ToArray()
            );
        }

        // Fallback: array simples para compatibilidade
        return result.Errors.Select(e => TranslatedMessage(e)).ToArray();
    }

    /// <summary>
    /// Formata erros para resposta padronizada (versão genérica).
    /// Se FieldErrors estiver disponível e populado, retorna dicionário por campo.
    /// Caso contrário, retorna array simples para compatibilidade retroativa.
    /// </summary>
    protected object FormatErrors<T>(ApplicationResult<T> result)
    {
        if (result.FieldErrors != null && result.FieldErrors.Count > 0)
        {
            // Formato padronizado: erros organizados por campo
            return result.FieldErrors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(e => TranslatedMessage(e)).ToArray()
            );
        }

        // Fallback: array simples para compatibilidade
        return result.Errors.Select(e => TranslatedMessage(e)).ToArray();
    }

    /// <summary>
    /// Verifica se há erros no ApplicationResult
    /// </summary>
    protected bool HasErrors(ApplicationResult result)
    {
        return (result.FieldErrors != null && result.FieldErrors.Count > 0) ||
               (result.Errors != null && result.Errors.Count > 0);
    }

    /// <summary>
    /// Verifica se há erros no ApplicationResult genérico
    /// </summary>
    protected bool HasErrors<T>(ApplicationResult<T> result)
    {
        return (result.FieldErrors != null && result.FieldErrors.Count > 0) ||
               (result.Errors != null && result.Errors.Count > 0);
    }

    /// <summary>
    /// Cria objeto de resposta de erro, incluindo errors apenas se houver erros
    /// </summary>
    protected object CreateErrorResponse(ApplicationResult result)
    {
        if (HasErrors(result))
        {
            return new
            {
                message = TranslatedMessage(result.Message),
                errors = FormatErrors(result)
            };
        }

        return new { message = TranslatedMessage(result.Message) };
    }

    /// <summary>
    /// Cria objeto de resposta de erro, incluindo errors apenas se houver erros (versão genérica)
    /// </summary>
    protected object CreateErrorResponse<T>(ApplicationResult<T> result)
    {
        if (HasErrors(result))
        {
            return new
            {
                message = TranslatedMessage(result.Message),
                errors = FormatErrors(result)
            };
        }

        return new { message = TranslatedMessage(result.Message) };
    }

    /// <summary>
    /// Mapeia um ApplicationResult genérico para ActionResult apropriado
    /// </summary>
    protected ActionResult<T> ToActionResult<T>(ApplicationResult<T> result)
    {
        if (!result.IsSuccess)
        {
            // Para operações que retornam dados, erros geralmente são BadRequest
            // a menos que seja uma busca específica (404)
            return BadRequest(CreateErrorResponse(result));
        }

        if (result.Data == null)
        {
            return NotFound(new
            {
                message = TranslatedMessage("NotFound")
            });
        }

        return result.Data;
    }

    /// <summary>
    /// Mapeia um ApplicationResult não genérico para IActionResult apropriado
    /// </summary>
    protected IActionResult ToActionResult(ApplicationResult result)
    {
        if (!result.IsSuccess)
        {
            return BadRequest(CreateErrorResponse(result));
        }

        return Ok(new { message = TranslatedMessage(result.Message) });
    }

    /// <summary>
    /// Mapeia ApplicationResult para CreatedAtAction (201 Created)
    /// </summary>
    protected ActionResult<T> ToCreatedAtAction<T>(ApplicationResult<T> result, string actionName, object? routeValues)
    {
        if (!result.IsSuccess || result.Data == null)
        {
            return BadRequest(CreateErrorResponse(result));
        }

        var createdAtActionResult = CreatedAtAction(actionName, routeValues, result.Data);
        return createdAtActionResult as ActionResult<T> ?? result.Data!;
    }

    /// <summary>
    /// Mapeia ApplicationResult para Ok (200 OK) com mensagem de sucesso traduzida
    /// </summary>
    protected IActionResult ToNoContent(ApplicationResult result)
    {
        if (!result.IsSuccess)
        {
            return NotFound(CreateErrorResponse(result));
        }

        return Ok(new { message = TranslatedMessage(result.Message) });
    }

    /// <summary>
    /// Mapeia ApplicationResult<T> para NotFound quando não encontrado, BadRequest para erros de validação
    /// </summary>
    protected ActionResult<T> ToActionResultWithNotFound<T>(ApplicationResult<T> result, string? notFoundMessage = null)
    {
        if (!result.IsSuccess)
        {
            // Se a mensagem indica "não encontrado", retorna 404
            if (result.Message.Contains("NotFound", StringComparison.OrdinalIgnoreCase) ||
                result.Message.Contains("Not Found", StringComparison.OrdinalIgnoreCase))
            {
                if (HasErrors(result))
                {
                    return NotFound(new
                    {
                        message = TranslatedMessage(notFoundMessage ?? result.Message),
                        errors = FormatErrors(result)
                    });
                }
                return NotFound(new
                {
                    message = TranslatedMessage(notFoundMessage ?? result.Message)
                });
            }

            // Caso contrário, BadRequest
            return BadRequest(CreateErrorResponse(result));
        }

        if (result.Data == null)
        {
            return NotFound(new
            {
                message = TranslatedMessage(notFoundMessage ?? "NotFound")
            });
        }

        return result.Data;
    }

    /// <summary>
    /// Retorna resposta de erro não autorizado
    /// </summary>
    protected IActionResult UnauthorizedResult(string messageKey)
    {
        return Unauthorized(new
        {
            message = TranslatedMessage(messageKey)
        });
    }
}

