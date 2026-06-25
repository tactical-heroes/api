using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new();
    private readonly List<FunctionalTestWebApplicationFactory> _factories = [];
    private readonly List<HttpClient> _clients = [];

    private FunctionalTestWebApplicationFactory _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    internal CapturingEventBus EventBus => _factory.EventBus;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();

        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _database.PostgreSqlConnectionString);

        CreateCurrentClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        EventBus.Clear();

        return _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    public void RestartApplication()
    {
        CreateCurrentClient();
    }

    private void CreateCurrentClient()
    {
        _factory = new FunctionalTestWebApplicationFactory();
        _factories.Add(_factory);

        Client = _factory.CreateClient();
        _clients.Add(Client);
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
    }
}
