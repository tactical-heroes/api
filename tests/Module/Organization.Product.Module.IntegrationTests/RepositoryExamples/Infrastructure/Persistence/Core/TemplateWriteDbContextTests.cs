using Npgsql;

namespace Organization.Product.Module.IntegrationTests.Infrastructure.Persistence.Core;

public sealed class TemplateWriteDbContextTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "MigrateAsync should create users table and indexes when migrations are applied")]
    public async Task MigrateAsync_Should_Create_Users_Table_And_Indexes_When_Migrations_Are_Applied()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var connection = new NpgsqlConnection(Fixture.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        var usersTableExists = await ExistsAsync(
            connection,
            """
            SELECT EXISTS (
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = 'public'
                  AND table_name = 'users'
            );
            """,
            cancellationToken);
        var emailIndexExists = await ExistsAsync(
            connection,
            """
            SELECT EXISTS (
                SELECT 1
                FROM pg_indexes
                WHERE schemaname = 'public'
                  AND tablename = 'users'
                  AND indexname = 'ix_users_email'
            );
            """,
            cancellationToken);
        var phoneIndexExists = await ExistsAsync(
            connection,
            """
            SELECT EXISTS (
                SELECT 1
                FROM pg_indexes
                WHERE schemaname = 'public'
                  AND tablename = 'users'
                  AND indexname = 'ix_users_phone'
            );
            """,
            cancellationToken);
        var birthDateColumnType = await GetStringAsync(
            connection,
            """
            SELECT data_type
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = 'users'
              AND column_name = 'birth_date';
            """,
            cancellationToken);

        usersTableExists.ShouldBeTrue();
        emailIndexExists.ShouldBeTrue();
        phoneIndexExists.ShouldBeTrue();
        birthDateColumnType.ShouldBe("date");
    }

    private static async Task<bool> ExistsAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);

        return (bool)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task<string> GetStringAsync(
        NpgsqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);

        return (string)(await command.ExecuteScalarAsync(cancellationToken))!;
    }
}
