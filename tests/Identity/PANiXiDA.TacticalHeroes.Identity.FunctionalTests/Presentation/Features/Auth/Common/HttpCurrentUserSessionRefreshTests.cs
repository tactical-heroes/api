using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Abstractions;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Common;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation.Features.Auth.Common;

public sealed class HttpCurrentUserSessionRefreshTests
{
    [Theory(DisplayName = "Session refresh should signal only successful responses when operation uses another scope")]
    [InlineData(204, true)]
    [InlineData(400, false)]
    [InlineData(500, false)]
    public async Task RequireRefresh_Should_SignalOnlySuccessfulResponses_When_OperationUsesAnotherScope(
        int statusCode, bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var authorizationId = Guid.CreateVersion7().ToString();
        var builder = WebApplication.CreateSlimBuilder();
        builder.Logging.ClearProviders();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserSessionRefresh, HttpCurrentUserSessionRefresh>();
        await using var app = builder.Build();
        app.Urls.Add("http://127.0.0.1:0");
        app.MapPost(
            "/arbitrary-operation",
            async (HttpContext context) =>
            {
                await using var scope = context.RequestServices.CreateAsyncScope();
                var refresh = scope.ServiceProvider.GetRequiredService<ICurrentUserSessionRefresh>();
                refresh.RequireRefresh(authorizationId);
                refresh.RequireRefresh(authorizationId);
                HttpCurrentUserSessionRefresh.GetAuthorizationId(context).ShouldBe(authorizationId);
                context.Response.StatusCode = statusCode;
            });
        app.MapGet(
            "/unrelated-request",
            (HttpContext context) =>
            {
                HttpCurrentUserSessionRefresh.GetAuthorizationId(context).ShouldBeNull();
                return Results.NoContent();
            });
        await app.StartAsync(cancellationToken);
        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };

        using var response = await client.PostAsync("/arbitrary-operation", content: null, cancellationToken);
        using var unrelated = await client.GetAsync("/unrelated-request", cancellationToken);

        ((int)response.StatusCode).ShouldBe(statusCode);
        response.Headers.Contains("X-Session-Refresh-Required").ShouldBe(expected);
        if (expected)
        {
            response.Headers.GetValues("X-Session-Refresh-Required").ShouldBe(["true"]);
        }
        unrelated.Headers.Contains("X-Session-Refresh-Required").ShouldBeFalse();
    }
}
