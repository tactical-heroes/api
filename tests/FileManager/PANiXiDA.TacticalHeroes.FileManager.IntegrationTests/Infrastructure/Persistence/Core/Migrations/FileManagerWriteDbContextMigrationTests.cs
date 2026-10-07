using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class FileManagerWriteDbContextMigrationTests(IntegrationTestFixture fixture)
{
    [Theory(DisplayName = "Read database model should match PostgreSQL columns when migrations are applied")]
    [InlineData("files")]
    [InlineData("folders")]
    public async Task ReadDbModel_Should_MatchPostgreSqlColumns_When_MigrationsAreApplied(string tableName)
    {
        await using var scope = fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<FileManagerReadDbContext>();
        var table = context.Model.GetRelationalModel().FindTable(tableName, "file_manager")!;
        var expected = table.Columns
            .Select(column => (column.Name, column.IsNullable))
            .OrderBy(column => column.Name, StringComparer.Ordinal)
            .ToArray();
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand(
            """
            SELECT attname, NOT attnotnull
            FROM pg_attribute
            WHERE attrelid = @table::regclass
              AND attnum > 0
              AND NOT attisdropped;
            """,
            connection);
        command.Parameters.AddWithValue("table", $"file_manager.{tableName}");
        var actual = new List<(string Name, bool IsNullable)>();

        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            actual.Add((reader.GetString(0), reader.GetBoolean(1)));
        }

        actual.OrderBy(column => column.Name, StringComparer.Ordinal).ShouldBe(expected);
    }

    [Fact(DisplayName = "File manager migrations should match the model and be repeatable when migrations are applied again")]
    public async Task MigrateAsync_Should_MatchModel_When_MigrationsAreAppliedAgain()
    {
        await using var scope = fixture.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();

        await dbContext.Database.MigrateAsync(TestContext.Current.CancellationToken);
        var applied = await dbContext.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
        var pending = await dbContext.Database.GetPendingMigrationsAsync(TestContext.Current.CancellationToken);

        applied.ShouldNotBeEmpty();
        pending.ShouldBeEmpty();
        dbContext.Database.HasPendingModelChanges().ShouldBeFalse();
    }

    [Fact(DisplayName = "File manager migrations should isolate tables when database is initialized")]
    public async Task Migrations_Should_UseFileManagerSchema_When_DatabaseIsInitialized()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);
        await using var command = new NpgsqlCommand(
            """
            SELECT schemaname || '.' || tablename
            FROM pg_tables
            WHERE schemaname IN ('file_manager', 'public')
            ORDER BY schemaname, tablename;
            """,
            connection);
        var tables = new List<string>();

        await using var reader = await command.ExecuteReaderAsync(TestContext.Current.CancellationToken);
        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            tables.Add(reader.GetString(0));
        }

        tables.ShouldBe(["file_manager.__EFMigrationsHistory", "file_manager.files", "file_manager.folders"]);
    }
}
