using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests;

public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlTestDatabase _database = new("file_manager");

    private ServiceProvider? _serviceProvider;

    public string ConnectionString => _database.PostgreSqlConnectionString;

    public AsyncServiceScope CreateScope()
    {
        return _serviceProvider!.CreateAsyncScope();
    }

    public Task ResetDatabaseAsync(CancellationToken cancellationToken)
    {
        return _database.ResetPostgreSqlDatabaseAsync(cancellationToken);
    }

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync(TestContext.Current.CancellationToken);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable.Replace(
                    "__",
                    ConfigurationPath.KeyDelimiter,
                    StringComparison.Ordinal)] = ConnectionString,
                ["FileManager:AWS:ServiceURL"] = "https://s3.example.invalid",
                ["FileManager:AWS:AuthenticationRegion"] = "us-east-1",
                ["FileManager:AWS:ForcePathStyle"] = "true",
                ["FileManager:S3Storage:BucketName"] = "file-manager-tests",
                ["FileManager:S3Storage:KeyPrefix"] = "integration-tests",
                ["FileManager:S3Storage:AccessKey"] = "test-access-key",
                ["FileManager:S3Storage:SecretKey"] = "test-secret-key"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true
            });

        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
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
