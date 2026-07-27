using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase database = new();

    private ServiceProvider? serviceProvider;

    public string ConnectionString => database.PostgreSqlConnectionString;

    public async ValueTask InitializeAsync()
    {
        await database.InitializeAsync(TestContext.Current.CancellationToken);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable.Replace(
                    "__",
                    ConfigurationPath.KeyDelimiter,
                    StringComparison.Ordinal)] = ConnectionString
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true
            });

        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CompendiumWriteDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (serviceProvider is not null)
        {
            await serviceProvider.DisposeAsync();
        }

        await database.DisposeAsync();
    }
}
