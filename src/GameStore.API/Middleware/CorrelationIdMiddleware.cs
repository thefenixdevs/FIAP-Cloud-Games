using System.Diagnostics;

namespace GameStore.API.Middleware;

public class CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<CorrelationIdMiddleware> _logger = logger;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
        {
            context.Response.Headers.Append(CorrelationIdHeaderName, correlationId);
        }

        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Request started: {Method} {Path} - CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms - CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId)
            && !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId.ToString();
        }

        return Guid.NewGuid().ToString();
    }
}
