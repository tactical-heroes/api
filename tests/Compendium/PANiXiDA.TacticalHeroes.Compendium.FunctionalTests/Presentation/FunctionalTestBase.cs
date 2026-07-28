namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

[Collection(FunctionalTestCollection.Name)]
public abstract class FunctionalTestBase(FunctionalTestFixture fixture)
    : IAsyncLifetime
{
    protected FunctionalTestFixture Fixture { get; } = fixture;

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
