using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Organization.Product.Module.Infrastructure.Persistence.Core;
using Organization.Product.Testing.Databases;

namespace Organization.Product.Module.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new();

    private FunctionalTestWebApplicationFactory _factory = null!;

    public HttpClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync();

        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _database.PostgreSqlConnectionString);

        _factory = new FunctionalTestWebApplicationFactory();
        Client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TemplateWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        return _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _factory.DisposeAsync();
        await _database.DisposeAsync();
    }
}
