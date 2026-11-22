using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Infrastructure.Persistence.Core.Migrations;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class BuildsWriteDbContextMigrationTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Migrations should create Builds history and HiLo sequence when applied")]
    public async Task Migrations_Should_CreateBuildsHistoryAndHiLoSequence_When_Applied()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var readContext = scope.ServiceProvider.GetRequiredService<BuildsReadDbContext>();

        var tables = await readContext.Database.SqlQuery<string>(
            $"""
            SELECT tablename AS "Value"
            FROM pg_tables
            WHERE schemaname = {"builds"}
            ORDER BY tablename
            """).ToListAsync(cancellationToken);
        var sequences = await readContext.Database.SqlQuery<string>(
            $"""
            SELECT sequence_name AS "Value"
            FROM information_schema.sequences
            WHERE sequence_schema = {"builds"}
            ORDER BY sequence_name
            """).ToListAsync(cancellationToken);

        tables.ShouldBe(["__EFMigrationsHistory", "build_heroes", "build_units", "builds"]);
        sequences.ShouldBe(["EntityFrameworkHiLoSequence"]);
        readContext.ChangeTracker.QueryTrackingBehavior.ShouldBe(QueryTrackingBehavior.NoTracking);
    }

    [Fact(DisplayName = "Migrations should preserve history and match the model when applied again")]
    public async Task Migrations_Should_PreserveHistoryAndMatchModel_When_AppliedAgain()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var writeContext = scope.ServiceProvider.GetRequiredService<BuildsWriteDbContext>();
        var appliedMigrations = await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken);

        await writeContext.Database.MigrateAsync(cancellationToken);

        appliedMigrations.ShouldNotBeEmpty();
        (await writeContext.Database.GetAppliedMigrationsAsync(cancellationToken)).ShouldBe(appliedMigrations);
        (await writeContext.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        writeContext.Database.HasPendingModelChanges().ShouldBeFalse();
    }
}
