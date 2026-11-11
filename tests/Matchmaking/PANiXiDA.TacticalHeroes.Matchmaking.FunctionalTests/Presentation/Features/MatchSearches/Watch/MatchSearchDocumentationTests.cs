using System.Text.Json;

using Microsoft.Extensions.Configuration;

using PANiXiDA.TacticalHeroes.Matchmaking.Presentation.Features.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Watch;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class MatchSearchDocumentationTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should publish the matchmaking HTTP and SignalR documents when enabled")]
    public async Task Get_Should_PublishMatchmakingDocumentation_When_Enabled()
    {
        const string SignalRDocument = "/asyncapi/matchmaking-signalr-v1.json";
        const string HttpDocument = "/openapi/matchmaking-v1.json";
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var factory = fixture.Factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["ScalarConfiguration:Enabled"] = "true" })));
        using var client = factory.CreateClient();

        var scalar = await client.GetStringAsync("/scalar", cancellationToken);
        using var openApi = await client.GetAsync(HttpDocument, cancellationToken);
        using var asyncApi = JsonDocument.Parse(await client.GetStringAsync(SignalRDocument, cancellationToken));

        scalar.ShouldContain(HttpDocument.TrimStart('/'));
        scalar.ShouldContain(SignalRDocument.TrimStart('/'));
        openApi.StatusCode.ShouldBe(HttpStatusCode.OK);
        var document = asyncApi.RootElement;
        document.GetProperty("channels").EnumerateObject().ShouldHaveSingleItem().Name.ShouldBe(MatchSearchHub.Route);
        var operation = document.GetProperty("operations").EnumerateObject().ShouldHaveSingleItem().Value;
        operation.GetProperty("action").GetString().ShouldBe("send");
        operation.GetProperty("summary").GetString()
            .ShouldBe("Sends the current match search state to the connected player.");
        operation.GetProperty("bindings").GetProperty("x-signalr").GetProperty("target").GetString()
            .ShouldBe(nameof(IMatchSearchClient.StateChangedAsync));
        var message = document.GetProperty("components").GetProperty("messages").EnumerateObject()
            .ShouldHaveSingleItem()
            .Value;
        var arguments = message.GetProperty("payload").GetProperty("schema").GetProperty("properties")
            .GetProperty("arguments");
        arguments.GetProperty("minItems").GetInt32().ShouldBe(1);
        arguments.GetProperty("prefixItems")[0].GetProperty("properties")
            .EnumerateObject()
            .Select(property => property.Name)
            .ShouldBe(["id", "status", "gameId", "queuedAt", "expiresAt"], ignoreOrder: true);
    }
}
