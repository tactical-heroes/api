using System.Collections.Concurrent;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.OAuth;

public sealed class OAuthRequestLoggingTests(FunctionalTestFixture fixture) : FunctionalTestBase(fixture)
{
    [Theory(DisplayName = "OAuth requests should log handled failures once when request body read fails")]
    [InlineData("/connect/token", FailureKind.TruncatedBody, StatusCodes.Status400BadRequest, LogLevel.Warning)]
    [InlineData("/connect/par", FailureKind.TruncatedBody, StatusCodes.Status400BadRequest, LogLevel.Warning)]
    [InlineData("/connect/introspect", FailureKind.TruncatedBody, StatusCodes.Status400BadRequest, LogLevel.Warning)]
    [InlineData("/connect/token", FailureKind.ClientCancellation, StatusCodes.Status499ClientClosedRequest, LogLevel.Warning)]
    [InlineData("/connect/par", FailureKind.ClientCancellation, StatusCodes.Status499ClientClosedRequest, LogLevel.Warning)]
    [InlineData("/connect/introspect", FailureKind.ClientCancellation, StatusCodes.Status499ClientClosedRequest, LogLevel.Warning)]
    [InlineData("/connect/token", FailureKind.ServerFailure, StatusCodes.Status500InternalServerError, LogLevel.Error)]
    public async Task PostOAuth_Should_LogHandledFailureOnce_When_RequestBodyReadFails(
        string path,
        FailureKind failureKind,
        int expectedStatus,
        LogLevel expectedLevel)
    {
        using var loggerProvider = new CapturingLoggerProvider();
        await using var factory = new FunctionalTestWebApplicationFactory("Development")
            .WithWebHostBuilder(builder => builder.ConfigureLogging(logging => logging.AddProvider(loggerProvider)));
        using var client = factory.CreateClient();
        var requestAborted = new CancellationToken(failureKind == FailureKind.ClientCancellation);
        Exception exception = failureKind switch
        {
            FailureKind.TruncatedBody => new BadHttpRequestException("Unexpected end of request content."),
            FailureKind.ClientCancellation => new OperationCanceledException(requestAborted),
            _ => new InvalidOperationException("Independent authentication failure")
        };
        await using var body = new FailingRequestBodyStream(exception);

        var response = await factory.Server.SendAsync(context =>
        {
            context.Request.Method = HttpMethods.Post;
            context.Request.Scheme = "https";
            context.Request.Host = new HostString("localhost");
            context.Request.Path = path;
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.ContentLength = 100;
            context.Request.Body = body;
            context.RequestAborted = requestAborted;
        }, TestContext.Current.CancellationToken);

        await loggerProvider.RequestCompleted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);

        response.Response.StatusCode.ShouldBe(expectedStatus);
        var completion = loggerProvider.Records
            .Where(record => record.Message == "HTTP request finished")
            .ShouldHaveSingleItem();
        completion.Level.ShouldBe(expectedLevel);
        completion.Attributes["url.path"].ShouldBe(path);
        completion.Attributes["http.request.method"].ShouldBe(HttpMethods.Post);
        completion.Attributes["http.response.status_code"].ShouldBe(expectedStatus);
        loggerProvider.Records.Count(record => record.Level >= LogLevel.Error)
            .ShouldBe(expectedLevel == LogLevel.Error ? 1 : 0);
        if (failureKind == FailureKind.ClientCancellation)
        {
            completion.Exception.ShouldBeNull();
        }
        else
        {
            completion.Exception.ShouldBeSameAs(exception);
        }
    }

    public enum FailureKind
    {
        TruncatedBody,
        ClientCancellation,
        ServerFailure
    }

    private sealed class FailingRequestBodyStream(Exception exception) : MemoryStream
    {
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
        {
            return ValueTask.FromException<int>(exception);
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private IExternalScopeProvider _scopeProvider = new LoggerExternalScopeProvider();

        internal ConcurrentQueue<LogEntry> Records { get; } = new();
        internal TaskCompletionSource RequestCompleted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(this);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) => _scopeProvider = scopeProvider;

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(CapturingLoggerProvider provider) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => provider._scopeProvider.Push(state);

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var attributes = new Dictionary<string, object?>();
                provider._scopeProvider.ForEachScope(static (scope, values) =>
                {
                    if (scope is IEnumerable<KeyValuePair<string, object?>> pairs)
                    {
                        foreach (var pair in pairs)
                        {
                            values.Add(pair.Key, pair.Value);
                        }
                    }
                }, attributes);
                var message = formatter(state, exception);
                provider.Records.Enqueue(new LogEntry(logLevel, message, exception, attributes));
                if (message == "HTTP request finished")
                {
                    provider.RequestCompleted.TrySetResult();
                }
            }
        }
    }

    private sealed record LogEntry(LogLevel Level, string Message, Exception? Exception, Dictionary<string, object?> Attributes);
}
