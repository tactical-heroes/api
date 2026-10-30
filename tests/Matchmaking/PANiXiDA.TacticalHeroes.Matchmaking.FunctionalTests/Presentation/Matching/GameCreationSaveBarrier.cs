using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

public sealed class GameCreationSaveBarrier : SaveChangesInterceptor
{
    private TaskCompletionSource? _reached;
    private TaskCompletionSource? _release;
    private bool _fail;

    public Task PauseNextSave(bool fail)
    {
        _fail = fail;
        _release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref _reached, reached);
        return reached.Task;
    }

    public void Resume()
    {
        Volatile.Read(ref _release)?.TrySetResult();
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        if (eventData.Context is not GamesWriteDbContext context
            || !context.ChangeTracker.Entries<Game>().Any(entry => entry.State == EntityState.Added))
        {
            return result;
        }

        var reached = Interlocked.Exchange(ref _reached, null);
        if (reached is null)
        {
            return result;
        }

        reached.TrySetResult();
        await Volatile.Read(ref _release)!.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);

        if (_fail)
        {
            throw new InvalidOperationException("Game persistence failed.");
        }

        return result;
    }
}
