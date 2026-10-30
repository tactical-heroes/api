using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.DependencyInjection;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Testing.Databases;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation;

public class FunctionalTestFixture : IAsyncLifetime
{
    private const string ScheduleEnabledVariable = "Matchmaking__Schedules__ProcessMatchmakingCommand__Enabled";
    private const string ExpirationEnabledVariable = "Matchmaking__Schedules__ExpireMatchSearchesCommand__Enabled";
    private const string FileManagerScheduleVariable = "FileManager__Schedules__ReconcileFileUploadsCommand__Enabled";

    private readonly PostgreSqlTestDatabase _database = new("matchmaking", isolateDatabase: true);

    private FunctionalTestWebApplicationFactory? _factory;
    private string? _previousConnectionString;
    private string? _previousScheduleEnabled;
    private string? _previousExpirationEnabled;
    private string? _previousFileManagerSchedule;

    public HttpClient Client { get; private set; } = null!;
    public IServiceProvider Services => _factory!.Services;

    protected virtual bool MatchmakingScheduleEnabled => false;
    protected virtual bool ExpirationScheduleEnabled => false;

    public async ValueTask InitializeAsync()
    {
        await _database.InitializeAsync(TestContext.Current.CancellationToken);

        _previousScheduleEnabled = Environment.GetEnvironmentVariable(ScheduleEnabledVariable);
        Environment.SetEnvironmentVariable(ScheduleEnabledVariable, MatchmakingScheduleEnabled.ToString());
        _previousExpirationEnabled = Environment.GetEnvironmentVariable(ExpirationEnabledVariable);
        Environment.SetEnvironmentVariable(ExpirationEnabledVariable, ExpirationScheduleEnabled.ToString());
        _previousFileManagerSchedule = Environment.GetEnvironmentVariable(FileManagerScheduleVariable);
        Environment.SetEnvironmentVariable(FileManagerScheduleVariable, "false");
        _previousConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable);
        Environment.SetEnvironmentVariable(
            PostgreSqlTestDatabase.PostgreSqlConnectionStringEnvironmentVariable,
            _database.PostgreSqlConnectionString);

        await MigrateDatabaseAsync(TestContext.Current.CancellationToken);

        _factory = new FunctionalTestWebApplicationFactory(
            matchmakingScheduleEnabled: MatchmakingScheduleEnabled,
            expirationScheduleEnabled: ExpirationScheduleEnabled);
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
        Environment.SetEnvironmentVariable(ScheduleEnabledVariable, _previousScheduleEnabled);
        Environment.SetEnvironmentVariable(ExpirationEnabledVariable, _previousExpirationEnabled);
        Environment.SetEnvironmentVariable(FileManagerScheduleVariable, _previousFileManagerSchedule);

        GC.SuppressFinalize(this);
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
        PANiXiDA.TacticalHeroes.Players.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        PANiXiDA.TacticalHeroes.Builds.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        PANiXiDA.TacticalHeroes.Games.Infrastructure.DependencyInjection.ServiceCollectionExtensions
            .AddInfrastructure(services, configuration);
        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext =
            scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);
        var playersDbContext = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();
        await playersDbContext.Database.MigrateAsync(cancellationToken);
        var buildsDbContext = scope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
        await buildsDbContext.Database.MigrateAsync(cancellationToken);
        var gamesDbContext = scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>();
        await gamesDbContext.Database.MigrateAsync(cancellationToken);
    }
}
