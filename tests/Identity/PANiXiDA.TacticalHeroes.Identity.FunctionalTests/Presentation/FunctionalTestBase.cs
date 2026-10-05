using System.Text.Json;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

[Collection(FunctionalTestCollection.Name)]
public abstract class FunctionalTestBase(FunctionalTestFixture fixture) : IAsyncLifetime
{
    protected static JsonSerializerOptions JsonOptions => TestJsonSerializerOptions.Web;

    protected FunctionalTestFixture Fixture { get; } = fixture;

    protected HttpClient Client => Fixture.Client;

    public async ValueTask InitializeAsync()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        await Fixture.ResetDatabaseAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
