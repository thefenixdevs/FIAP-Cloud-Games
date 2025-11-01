using FluentValidation;
using GameStore.Application.Common.Results;
using Mediator;
using Microsoft.Extensions.Logging;

namespace GameStore.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(request, cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count != 0)
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {Errors}",
                typeof(TRequest).Name,
                string.Join(", ", failures.Select(f => f.ErrorMessage)));

            // Criar resposta de erro usando ApplicationResult
            var errorResponse = CreateErrorResponse(failures);
            
            if (errorResponse != null)
            {
                return errorResponse;
            }

            throw new ValidationException(failures);
        }

        return await next(request, cancellationToken);
    }

    private TResponse? CreateErrorResponse(List<FluentValidation.Results.ValidationFailure> failures)
    {
        var responseType = typeof(TResponse);
        
        // Agrupar erros por PropertyName (nome do campo) para facilitar tratamento no frontend
        var fieldErrors = failures
            .GroupBy(f => f.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(f => f.ErrorMessage).ToArray()
            );

        // Verificar se TResponse é ApplicationResult<T> ou ApplicationResult
        if (responseType.IsGenericType)
        {
            var genericTypeDefinition = responseType.GetGenericTypeDefinition();
            
            // ApplicationResult<T>
            if (genericTypeDefinition == typeof(ApplicationResult<>))
            {
                var combinedMessage = "ValidationFailed";
                
                // Tentar usar o método ValidationFailure que aceita Dictionary<string, string[]>
                var fieldErrorsMethod = responseType.GetMethod(
                    "ValidationFailure",
                    new[] { typeof(IReadOnlyDictionary<string, string[]>), typeof(string) });
                
                if (fieldErrorsMethod != null)
                {
                    try
                    {
                        var result = fieldErrorsMethod.Invoke(
                            null,
                            new object[] { fieldErrors, combinedMessage });
                        return (TResponse)result!;
                    }
                    catch
                    {
                        // Fallback para método antigo se falhar
                    }
                }

                // Fallback: tentar método com lista de erros (compatibilidade)
                var errorMessages = failures.Select(f => f.ErrorMessage).ToList();
                var validationFailureMethod = responseType.GetMethod(
                    "ValidationFailure",
                    new[] { typeof(IReadOnlyCollection<string>), typeof(string) });
                
                if (validationFailureMethod != null)
                {
                    try
                    {
                        var result = validationFailureMethod.Invoke(
                            null,
                            new object[] { errorMessages, combinedMessage });
                        return (TResponse)result!;
                    }
                    catch
                    {
                        return default;
                    }
                }
            }
        }
        else if (responseType == typeof(ApplicationResult))
        {
            // ApplicationResult não genérico
            var combinedMessage = "ValidationFailed";
            
            // Usar método que aceita Dictionary<string, string[]>
            var result = ApplicationResult.ValidationFailure(fieldErrors, combinedMessage);
            return (TResponse)(object)result;
        }

        return default;
    }
}

