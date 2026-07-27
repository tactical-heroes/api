using Npgsql;

namespace PANiXiDA.TacticalHeroes.Compendium.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

[Collection(IntegrationTestCollection.Name)]
public sealed class CompendiumWriteDbContextMigrationTests(
    IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Migrations should use the Compendium schema and history table")]
    public async Task Migrations_Should_UseCompendiumSchemaAndHistoryTable()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var compendiumTables = await ReadNamesAsync(
            connection,
            """
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = 'compendium'
            ORDER BY tablename;
            """);
        var publicTables = await ReadNamesAsync(
            connection,
            """
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = 'public'
            ORDER BY tablename;
            """);
        var compendiumSequences = await ReadNamesAsync(
            connection,
            """
            SELECT sequence_name
            FROM information_schema.sequences
            WHERE sequence_schema = 'compendium'
            ORDER BY sequence_name;
            """);

        compendiumTables.ShouldBe(["__EFMigrationsHistory", "factions"]);
        publicTables.ShouldBeEmpty();
        compendiumSequences.ShouldBe(["EntityFrameworkHiLoSequence"]);
    }

    private static async Task<string[]> ReadNamesAsync(
        NpgsqlConnection connection,
        string commandText)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;

        await using var reader = await command.ExecuteReaderAsync(
            TestContext.Current.CancellationToken);
        var names = new List<string>();

        while (await reader.ReadAsync(TestContext.Current.CancellationToken))
        {
            names.Add(reader.GetString(0));
        }

        return names.ToArray();
    }
}
