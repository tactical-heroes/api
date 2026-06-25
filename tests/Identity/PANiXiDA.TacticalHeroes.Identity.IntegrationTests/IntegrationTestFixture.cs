using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new();

    private ServiceProvider _serviceProvider = null!;

    private static string PostgreSqlConnectionStringConfigurationKey =>
        PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable.Replace(
            "__",
            ConfigurationPath.KeyDelimiter,
            StringComparison.Ordinal);

    public string ConnectionString => _database.PostgreSqlConnectionString;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [PostgreSqlConnectionStringConfigurationKey] = ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.RunWolverineInSoloMode();

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true
            });

        await using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public AsyncServiceScope CreateScope()
    {
        return _serviceProvider.CreateAsyncScope();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        return _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_serviceProvider is not null)
        {
            await _serviceProvider.DisposeAsync();
        }

        await _database.DisposeAsync();
    }
}
