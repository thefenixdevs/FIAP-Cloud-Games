using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using GameStore.API.Middleware;
using Moq;
using Xunit;

namespace GameStore.Tests.API.Middleware;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldGenerateCorrelationId_WhenHeaderNotProvided()
    {
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var context = new DefaultHttpContext();
        var nextCalled = false;

        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
        Assert.NotEmpty(context.Response.Headers["X-Correlation-Id"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseProvidedCorrelationId_WhenHeaderExists()
    {
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var context = new DefaultHttpContext();
        var expectedCorrelationId = "test-correlation-id-123";
        context.Request.Headers["X-Correlation-Id"] = expectedCorrelationId;

        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new CorrelationIdMiddleware(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedCorrelationId, context.Response.Headers["X-Correlation-Id"].ToString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequestStartAndCompletion()
    {
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/test";

        RequestDelegate next = (ctx) =>
        {
            ctx.Response.StatusCode = 200;
            return Task.CompletedTask;
        };

        var middleware = new CorrelationIdMiddleware(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request started")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogCompletion_EvenWhenExceptionThrown()
    {
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var context = new DefaultHttpContext();

        RequestDelegate next = (ctx) =>
        {
            throw new Exception("Test exception");
        };

        var middleware = new CorrelationIdMiddleware(next, loggerMock.Object);

        await Assert.ThrowsAsync<Exception>(async () => await middleware.InvokeAsync(context));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Request completed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddCorrelationIdToResponseHeaders()
    {
        var loggerMock = new Mock<ILogger<CorrelationIdMiddleware>>();
        var context = new DefaultHttpContext();

        RequestDelegate next = (ctx) => Task.CompletedTask;

        var middleware = new CorrelationIdMiddleware(next, loggerMock.Object);

        await middleware.InvokeAsync(context);

        Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id"));
    }
}
