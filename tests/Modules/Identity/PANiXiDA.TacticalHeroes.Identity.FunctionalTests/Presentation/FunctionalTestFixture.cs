using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core.Migrations;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new("identity", isolateDatabase: true, cacheResetPlan: true);
    private readonly List<FunctionalTestWebApplicationFactory> _factories = [];
    private readonly List<HttpClient> _clients = [];

    private readonly Dictionary<string, FunctionalTestWebApplicationFactory> _environmentFactories =
        new(StringComparer.Ordinal);

    private FunctionalTestWebApplicationFactory _factory = null!;
    private NpgsqlConnection? _seedConnection;
    private OpenIddictEntityFrameworkCoreApplication<Guid>[] _seedApplications = [];

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory.Services;

    internal CapturingEventBus EventBus => _factory.EventBus;

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await FunctionalTestWebApplicationFactory.InitializeDatabaseAsync(_database, cancellationToken);

        await MigrateDatabaseAsync(cancellationToken);
        CreateCurrentClient();

        await SeedIdentityProviderAsync(cancellationToken);
        await SnapshotSeedDataAsync(cancellationToken);
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        EventBus.Clear();

        await _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
        await using var command = _seedConnection!.CreateCommand();
        command.CommandText = """
            INSERT INTO identity.asp_net_roles SELECT * FROM seeded_roles;
            INSERT INTO identity.asp_net_role_claims SELECT * FROM seeded_role_claims;
            INSERT INTO identity.open_iddict_applications SELECT * FROM seeded_applications;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);

        foreach (var factory in _factories)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var cache = scope.ServiceProvider
                .GetRequiredService<IOpenIddictApplicationCache<OpenIddictEntityFrameworkCoreApplication<Guid>>>();
            foreach (var application in _seedApplications)
            {
                await cache.RemoveAsync(application, cancellationToken);
            }
        }
    }

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options)
    {
        var client = _factory.CreateClient(options);
        _clients.Add(client);

        return client;
    }

    public HttpClient CreateClient(string environmentName)
    {
        if (!_environmentFactories.TryGetValue(environmentName, out var factory))
        {
            factory = CreateFactory(environmentName);
            _environmentFactories.Add(environmentName, factory);
            _factories.Add(factory);
        }

        var client = factory.CreateClient();
        _clients.Add(client);

        return client;
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients)
        {
            client.Dispose();
        }

        foreach (var factory in _factories)
        {
            await factory.DisposeAsync();
        }

        if (_seedConnection is not null)
        {
            await _seedConnection.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    internal FunctionalTestWebApplicationFactory CreateFactory(string environmentName = "Test")
    {
        return new FunctionalTestWebApplicationFactory(_database.PostgreSqlConnectionString, environmentName);
    }

    private void CreateCurrentClient()
    {
        _factory = CreateFactory();
        _factories.Add(_factory);

        Client = _factory.CreateClient();
        _clients.Add(Client);
    }

    private async Task SeedIdentityProviderAsync(CancellationToken cancellationToken)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var migration = new SeedRolesAndClaims();
        var operations = migration.UpOperations.Concat(new RemoveNonPermissionClaims().UpOperations);
        foreach (var operation in operations.Cast<SqlOperation>())
        {
            await dbContext.Database.ExecuteSqlRawAsync(operation.Sql, cancellationToken);
        }
        var seeder = scope.ServiceProvider.GetRequiredService<IdentityProviderApplicationSeeder>();

        await seeder.SeedAsync(cancellationToken);
    }

    private async Task MigrateDatabaseAsync(CancellationToken cancellationToken)
    {
        var options = new DbContextOptionsBuilder<IdentityWriteDbContext>()
            .UseNpgsql(_database.PostgreSqlConnectionString)
            .Options;
        await using var dbContext = new IdentityWriteDbContext(options, []);

        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private async Task SnapshotSeedDataAsync(CancellationToken cancellationToken)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        _seedApplications = await dbContext.Set<OpenIddictEntityFrameworkCoreApplication<Guid>>()
            .AsNoTracking()
            .ToArrayAsync(cancellationToken);

        _seedConnection = new NpgsqlConnection(_database.PostgreSqlConnectionString);
        await _seedConnection.OpenAsync(cancellationToken);

        await using var command = _seedConnection.CreateCommand();
        command.CommandText = """
            CREATE TEMP TABLE seeded_roles AS TABLE identity.asp_net_roles;
            CREATE TEMP TABLE seeded_role_claims AS TABLE identity.asp_net_role_claims;
            CREATE TEMP TABLE seeded_applications AS TABLE identity.open_iddict_applications;
            """;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
