using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Compendium.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new("compendium");

    private FunctionalTestWebApplicationFactory? _factory;
    private string? _previousConnectionString;

    public HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync(TestContext.Current.CancellationToken);

        _previousConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _database.PostgreSqlConnectionString);

        await MigrateDatabaseAsync(TestContext.Current.CancellationToken);

        _factory = new FunctionalTestWebApplicationFactory();
        Client = _factory.CreateClient();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        return _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _database.DisposeAsync();

        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _previousConnectionString);
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
                [connectionStringKey] = _database.PostgreSqlConnectionString
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
}
