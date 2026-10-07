namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests;

[Collection(IntegrationTestCollectionDefinition.Name)]
public abstract class IntegrationTestBase(IntegrationTestFixture fixture)
    : IAsyncLifetime
{
    protected IntegrationTestFixture Fixture { get; } = fixture;

    public async ValueTask InitializeAsync()
    {
        await Fixture.ResetDatabaseAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}
