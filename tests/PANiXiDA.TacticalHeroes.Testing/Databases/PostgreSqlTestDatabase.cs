using Npgsql;

using Respawn;
using Respawn.Graph;

using Testcontainers.PostgreSql;

namespace PANiXiDA.TacticalHeroes.Testing.Databases;

public sealed class PostgreSqlTestDatabase(string schema, bool isolateDatabase = false) : IAsyncDisposable
{
    public const string PostgreSqlConnectionStringEnvironmentVariable =
        "ConnectionStrings__PostgreSqlConnectionString";

    private PostgreSqlContainer? _postgreSqlContainer;
    private string? _postgreSqlConnectionString;
    private Respawner? _respawner;
    private string? _administrationConnectionString;
    private string? _isolatedDatabaseName;

    public string PostgreSqlConnectionString =>
        _postgreSqlConnectionString ??
            throw new InvalidOperationException("PostgreSQL test database is not initialized.");

    public async ValueTask InitializeAsync(
        CancellationToken cancellationToken)
    {
        var configuredConnectionString = Environment.GetEnvironmentVariable(
            PostgreSqlConnectionStringEnvironmentVariable);

        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            _postgreSqlConnectionString = configuredConnectionString;

            if (isolateDatabase)
            {
                _isolatedDatabaseName = $"test_{Guid.NewGuid():N}";
                await using var connection = new NpgsqlConnection(configuredConnectionString);
                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = $"CREATE DATABASE \"{_isolatedDatabaseName}\"";
                await command.ExecuteNonQueryAsync(cancellationToken);
                _administrationConnectionString = configuredConnectionString;
                _postgreSqlConnectionString = new NpgsqlConnectionStringBuilder(configuredConnectionString)
                {
                    Database = _isolatedDatabaseName
                }.ConnectionString;
            }

            return;
        }

        _postgreSqlContainer = new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("organization_product_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await _postgreSqlContainer.StartAsync(cancellationToken);
        _postgreSqlConnectionString = _postgreSqlContainer.GetConnectionString();
    }

    public async Task ResetPostgreSqlDatabaseAsync(
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(PostgreSqlConnectionString);
        await connection.OpenAsync(cancellationToken);

        _respawner ??= await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = [schema],
                TablesToIgnore =
                [
                    new Table(schema, "__ef_migrations_history"),
                    new Table(schema, "__EFMigrationsHistory")
                ]
            });

        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        if (_administrationConnectionString is not null)
        {
            await using var databaseConnection = new NpgsqlConnection(PostgreSqlConnectionString);
            NpgsqlConnection.ClearPool(databaseConnection);

            await using var connection = new NpgsqlConnection(_administrationConnectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.CommandText = $"DROP DATABASE \"{_isolatedDatabaseName}\" WITH (FORCE)";
            await command.ExecuteNonQueryAsync();
        }

        if (_postgreSqlContainer is not null)
        {
            await _postgreSqlContainer.DisposeAsync();
        }
    }
}
