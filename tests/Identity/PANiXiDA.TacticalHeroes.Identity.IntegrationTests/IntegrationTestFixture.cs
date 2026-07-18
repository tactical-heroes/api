using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;
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

    public CommandCounterInterceptor CommandCounter { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [PostgreSqlConnectionStringConfigurationKey] = ConnectionString,
                [$"{IdentityProviderOptions.SectionName}:Issuer"] = "https://localhost/",
                [$"{IdentityProviderOptions.SectionName}:Audience"] = "tactical-heroes-api",
                [$"{IdentityProviderOptions.SectionName}:Clients:0:ClientId"] = "integration-tests",
                [$"{IdentityProviderOptions.SectionName}:Clients:0:DisplayName"] = "Integration Tests",
                [$"{IdentityProviderOptions.SectionName}:Clients:0:ClientType"] =
                    OpenIddictConstants.ClientTypes.Public,
                [$"{IdentityProviderOptions.SectionName}:Clients:0:GrantTypes:0"] =
                    OpenIddictConstants.GrantTypes.AuthorizationCode
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(CommandCounter);
        services.AddSingleton<IInterceptor>(CommandCounter);
        services.AddInfrastructure(configuration);
        services.RunWolverineInSoloMode();

        _serviceProvider = services.BuildServiceProvider(
            options: new ServiceProviderOptions
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
