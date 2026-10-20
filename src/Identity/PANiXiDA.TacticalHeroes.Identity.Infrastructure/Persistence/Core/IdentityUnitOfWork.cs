using Microsoft.EntityFrameworkCore;

using PANiXiDA.Core.Application.Persistence.Errors;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

internal sealed class IdentityUnitOfWork(IdentityWriteDbContext context) : IUnitOfWork
{
    private readonly EfUnitOfWork<IdentityWriteDbContext> _unitOfWork = new(context);

    public bool HasActiveTransaction => _unitOfWork.HasActiveTransaction;

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return TranslateConcurrencyAsync(_unitOfWork.SaveChangesAsync(cancellationToken));
    }

    public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
    {
        return TranslateConcurrencyAsync(_unitOfWork.ExecuteInTransactionAsync(action, cancellationToken));
    }

    public Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return _unitOfWork.BeginTransactionAsync(cancellationToken);
    }

    public Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        return _unitOfWork.CommitTransactionAsync(cancellationToken);
    }

    public Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        return _unitOfWork.RollbackTransactionAsync(cancellationToken);
    }

    public ValueTask DisposeTransactionAsync()
    {
        return _unitOfWork.DisposeTransactionAsync();
    }

    private static async Task TranslateConcurrencyAsync(Task operation)
    {
        try
        {
            await operation;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyException("The entity was modified or deleted by another operation.", exception);
        }
    }
}
