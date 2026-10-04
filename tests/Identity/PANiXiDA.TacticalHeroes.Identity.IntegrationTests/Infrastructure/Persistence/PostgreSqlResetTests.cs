using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class PostgreSqlResetTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Database reset should preserve other schemas when module data exists")]
    public async Task ResetDatabase_Should_PreserveOtherSchemas_When_ModuleDataExists()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var connection = new NpgsqlConnection(Fixture.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using var setup = new NpgsqlCommand("""
            CREATE SCHEMA IF NOT EXISTS wolverine;
            CREATE TABLE wolverine.reset_isolation_probe (id integer PRIMARY KEY);
            CREATE TABLE identity.reset_isolation_probe (id integer PRIMARY KEY);
            INSERT INTO wolverine.reset_isolation_probe VALUES (1);
            INSERT INTO identity.reset_isolation_probe VALUES (1);
            """, connection);
        await setup.ExecuteNonQueryAsync(cancellationToken);

        try
        {
            await Fixture.ResetDatabaseAsync(cancellationToken);

            await using var moduleRows = new NpgsqlCommand(
                "SELECT count(*) FROM identity.reset_isolation_probe", connection);
            (await moduleRows.ExecuteScalarAsync(cancellationToken)).ShouldBe(0L);
            await using var messagingRows = new NpgsqlCommand(
                "SELECT count(*) FROM wolverine.reset_isolation_probe", connection);
            (await messagingRows.ExecuteScalarAsync(cancellationToken)).ShouldBe(1L);
        }
        finally
        {
            using var cleanupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await using var cleanup = new NpgsqlCommand("""
                DROP TABLE identity.reset_isolation_probe;
                DROP TABLE wolverine.reset_isolation_probe;
                """, connection);
            await cleanup.ExecuteNonQueryAsync(cleanupTimeout.Token);
        }
    }

    [Fact(DisplayName = "Database reset should preserve migration history when migrations have been applied")]
    public async Task ResetDatabase_Should_PreserveMigrationHistory_When_MigrationsHaveBeenApplied()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityWriteDbContext>();
        var expectedMigrations = dbContext.Database.GetMigrations().ToArray();

        await Fixture.ResetDatabaseAsync(cancellationToken);

        expectedMigrations.ShouldNotBeEmpty();
        var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync(cancellationToken);
        appliedMigrations.ShouldBe(expectedMigrations);
    }
}
