using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;
using PANiXiDA.TacticalHeroes.Testing.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.FunctionalTests.Presentation;

public sealed class FunctionalTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new("file_manager");
    private readonly Dictionary<string, string?> _previousEnvironment = [];

    private FunctionalTestWebApplicationFactory? _factory;

    public HttpClient Client { get; private set; } = null!;
    public S3TestStorage Storage { get; } = new();

    public AsyncServiceScope CreateScope()
    {
        return _factory!.Services.CreateAsyncScope();
    }

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _database.InitializeAsync(cancellationToken);
        await Storage.InitializeAsync(cancellationToken);

        var settings = Storage.GetConfiguration(nameof(FileManager));
        settings["ConnectionStrings:PostgreSqlConnectionString"] = _database.PostgreSqlConnectionString;
        foreach (var (key, value) in settings)
        {
            var variable = key.Replace(ConfigurationPath.KeyDelimiter, "__", StringComparison.Ordinal);
            _previousEnvironment[variable] = Environment.GetEnvironmentVariable(variable);
            Environment.SetEnvironmentVariable(variable, value);
        }

        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        await using (var serviceProvider = services.BuildServiceProvider())
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        _factory = new FunctionalTestWebApplicationFactory();
        Client = _factory.CreateClient();
    }

    public async Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        await _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
        await Storage.ResetAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _database.DisposeAsync();
        await Storage.DisposeAsync();

        foreach (var (variable, value) in _previousEnvironment)
        {
            Environment.SetEnvironmentVariable(variable, value);
        }
    }
}
