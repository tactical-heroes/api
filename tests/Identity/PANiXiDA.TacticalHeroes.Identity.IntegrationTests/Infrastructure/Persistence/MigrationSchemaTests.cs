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
            "open_iddict_applications",
            "open_iddict_authorizations",
            "open_iddict_scopes",
            "open_iddict_tokens",
            "role_claims",
            "roles",
            "user_claims",
            "user_confirmation_tokens",
            "user_password_reset_tokens",
            "user_roles",
            "users"
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
