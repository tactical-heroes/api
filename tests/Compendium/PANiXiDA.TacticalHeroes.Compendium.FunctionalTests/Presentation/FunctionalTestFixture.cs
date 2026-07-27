using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase database = new();

    private FunctionalTestWebApplicationFactory? factory;
    private string? previousConnectionString;

    public HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await database.InitializeAsync(TestContext.Current.CancellationToken);

        previousConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            database.PostgreSqlConnectionString);

        await MigrateDatabaseAsync(TestContext.Current.CancellationToken);

        factory = new FunctionalTestWebApplicationFactory();
        Client = factory.CreateClient();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        return database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    private async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
    {
        var connectionStringKey =
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable.Replace(
                "__",
                ConfigurationPath.KeyDelimiter,
                StringComparison.Ordinal);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [connectionStringKey] = database.PostgreSqlConnectionString
            })
            .Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await database.DisposeAsync();

        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            previousConnectionString);
    }
}
