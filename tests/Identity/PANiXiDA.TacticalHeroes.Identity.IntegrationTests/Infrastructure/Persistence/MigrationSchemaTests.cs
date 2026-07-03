using Npgsql;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Persistence;

public sealed class MigrationSchemaTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Migrations should create identity tables outside public schema")]
    public async Task Migrations_Should_CreateIdentityTablesOutsidePublicSchema()
    {
        var expectedIdentityTables = new[]
        {
            "__ef_migrations_history",
            "asp_net_role_claims",
            "asp_net_roles",
            "asp_net_user_claims",
            "asp_net_user_logins",
            "asp_net_user_roles",
            "asp_net_user_tokens",
            "asp_net_users",
            "data_protection_keys",
            "open_iddict_applications",
            "open_iddict_authorizations",
            "open_iddict_scopes",
            "open_iddict_tokens"
        };

        await using var connection = new NpgsqlConnection(Fixture.ConnectionString);
        await connection.OpenAsync(TestContext.Current.CancellationToken);

        var publicTables = await ReadTableNamesAsync(
            connection,
            "public",
            TestContext.Current.CancellationToken);
        var identityTables = await ReadTableNamesAsync(
            connection,
            "identity",
            TestContext.Current.CancellationToken);

        publicTables.ShouldBeEmpty();
        identityTables.ShouldBe(expectedIdentityTables, ignoreOrder: true);
    }

    private static async Task<string[]> ReadTableNamesAsync(
        NpgsqlConnection connection,
        string schema,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT tablename
            FROM pg_tables
            WHERE schemaname = @schema
            ORDER BY tablename;
            """;
        command.Parameters.AddWithValue("schema", schema);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var tableNames = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            tableNames.Add(reader.GetString(0));
        }

        return tableNames.ToArray();
    }
}
