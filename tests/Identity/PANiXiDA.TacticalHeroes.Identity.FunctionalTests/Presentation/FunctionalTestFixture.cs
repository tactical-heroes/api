using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.IdentityProvider.Seeding;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new();
    private readonly List<FunctionalTestWebApplicationFactory> _factories = [];
    private readonly List<HttpClient> _clients = [];

    private FunctionalTestWebApplicationFactory _factory = null!;
    private string? _previousConnectionString;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory.Services;

    internal CapturingEventBus EventBus => _factory.EventBus;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();

        _previousConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _database.PostgreSqlConnectionString);

        await MigrateDatabaseAsync(TestContext.Current.CancellationToken);
        CreateCurrentClient();

        await SeedIdentityProviderAsync(TestContext.Current.CancellationToken);
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        EventBus.Clear();

        await _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
        await SeedIdentityProviderAsync(cancellationToken);
    }

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options)
    {
        var client = _factory.CreateClient(options);
        _clients.Add(client);

        return client;
    }

    private void CreateCurrentClient()
    {
        _factory = new FunctionalTestWebApplicationFactory();
        _factories.Add(_factory);

        Client = _factory.CreateClient();
        _clients.Add(Client);
    }

    private async Task SeedIdentityProviderAsync(CancellationToken cancellationToken)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
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

        await _database.DisposeAsync();

        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _previousConnectionString);
    }
}
