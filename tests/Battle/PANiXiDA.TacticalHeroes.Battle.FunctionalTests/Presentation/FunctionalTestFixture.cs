using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Battle.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Battle.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new("battle", isolateDatabase: true);

    private FunctionalTestWebApplicationFactory? _factory;
    private string? _previousConnectionString;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory!.Services;

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
        PANiXiDA.TacticalHeroes.Games.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        PANiXiDA.TacticalHeroes.Builds.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<BattleWriteDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        var gamesContext = scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>();
        await gamesContext.Database.MigrateAsync(cancellationToken);
        var buildsContext = scope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
        await buildsContext.Database.MigrateAsync(cancellationToken);
        var compendiumContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        await compendiumContext.Database.MigrateAsync(cancellationToken);
    }
}
