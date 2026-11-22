using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.IntegrationTests.Infrastructure.Persistence.Features.Folders.Write;

public sealed class PostgreSqlFolderHierarchyLockTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Hierarchy lock should require a transaction when no transaction is active")]
    public async Task AcquireAsync_Should_RequireTransaction_When_NoTransactionIsActive()
    {
        await using var scope = Fixture.CreateScope();
        var hierarchyLock = scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>();

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            hierarchyLock.AcquireAsync(TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("active transaction");
    }

    [Theory(DisplayName = "Hierarchy lock should release waiting writers when commit or rollback completes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AcquireAsync_Should_ReleaseWaitingWriters_When_CommitOrRollbackCompletes(bool commit)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var firstScope = Fixture.CreateScope();
        var firstDb = firstScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var firstTransaction = await firstDb.Database.BeginTransactionAsync(ct);
        await firstScope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        await using var secondScope = Fixture.CreateScope();
        var secondDb = secondScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var secondTransaction = await secondDb.Database.BeginTransactionAsync(ct);
        var backendId = ((NpgsqlConnection)secondDb.Database.GetDbConnection()).ProcessID;
        var pending = secondScope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        bool waited;

        try
        {
            waited = await WaitForLockAsync(firstDb, backendId, ct);
        }
        finally
        {
            if (commit)
            {
                await firstTransaction.CommitAsync(ct);
            }
            else
            {
                await firstTransaction.RollbackAsync(ct);
            }
        }
        await pending.WaitAsync(TimeSpan.FromSeconds(10), ct);
        await secondTransaction.RollbackAsync(ct);

        waited.ShouldBeTrue();
        pending.IsCompletedSuccessfully.ShouldBeTrue();
    }

    [Theory(DisplayName = "Hierarchy lock should wait again when the same scope starts another transaction")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AcquireAsync_Should_WaitAgain_When_TheSameScopeStartsAnotherTransaction(bool commit)
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = Fixture.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        var hierarchyLock = scope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>();
        await using (var initial = await db.Database.BeginTransactionAsync(ct))
        {
            await hierarchyLock.AcquireAsync(ct);
            await hierarchyLock.AcquireAsync(ct);
            if (commit)
            {
                await initial.CommitAsync(ct);
            }
            else
            {
                await initial.RollbackAsync(ct);
            }
        }
        await using var competingScope = Fixture.CreateScope();
        var competingDb = competingScope.ServiceProvider.GetRequiredService<FileManagerWriteDbContext>();
        await using var competing = await competingDb.Database.BeginTransactionAsync(ct);
        await competingScope.ServiceProvider.GetRequiredService<IFolderHierarchyLock>().AcquireAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var backendId = ((NpgsqlConnection)db.Database.GetDbConnection()).ProcessID;
        var pending = hierarchyLock.AcquireAsync(ct);
        bool waited;

        try
        {
            waited = await WaitForLockAsync(competingDb, backendId, ct);
        }
        finally
        {
            await competing.RollbackAsync(ct);
        }
        await pending.WaitAsync(TimeSpan.FromSeconds(10), ct);
        await transaction.RollbackAsync(ct);

        waited.ShouldBeTrue();
        pending.IsCompletedSuccessfully.ShouldBeTrue();
    }

    private static async Task<bool> WaitForLockAsync(
        FileManagerWriteDbContext dbContext, int backendId, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);
        while (DateTime.UtcNow < deadline)
        {
            var waiting = await dbContext.Database.SqlQuery<bool>(
                $"""
                SELECT EXISTS (
                    SELECT 1 FROM pg_locks
                    WHERE pid = {backendId} AND locktype = 'advisory' AND NOT granted
                ) AS "Value"
                """)
                .SingleAsync(cancellationToken);
            if (waiting)
            {
                return true;
            }

            await Task.Delay(10, cancellationToken);
        }

        return false;
    }
}
