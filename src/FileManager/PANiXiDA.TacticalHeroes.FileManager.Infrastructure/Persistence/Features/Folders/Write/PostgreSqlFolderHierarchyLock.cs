using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;

public sealed class PostgreSqlFolderHierarchyLock(FileManagerWriteDbContext dbContext) : IFolderHierarchyLock
{
    private const string HierarchyLockName = $"{nameof(FileManager)}:FolderHierarchy";

    private Guid? _transactionId;

    public async Task AcquireAsync(CancellationToken cancellationToken)
    {
        var transaction = dbContext.Database.CurrentTransaction
            ?? throw new InvalidOperationException("Locking the folder hierarchy requires an active transaction.");
        cancellationToken.ThrowIfCancellationRequested();
        if (_transactionId == transaction.TransactionId)
        {
            return;
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({HierarchyLockName}, 0))", cancellationToken);
        _transactionId = transaction.TransactionId;
    }
}
