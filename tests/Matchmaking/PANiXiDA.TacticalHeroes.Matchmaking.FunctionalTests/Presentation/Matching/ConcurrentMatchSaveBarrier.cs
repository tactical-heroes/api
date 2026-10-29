using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

public sealed class ConcurrentMatchSaveBarrier : SaveChangesInterceptor
{
    private TaskCompletionSource? _release;
    private int _remaining;
    private TaskCompletionSource? _saveReached;
    private TaskCompletionSource? _saveRelease;

    public Task PauseNextMatchSave()
    {
        var reached = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(
            ref _saveRelease,
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        Interlocked.Exchange(ref _saveReached, reached);

        return reached.Task;
    }

    public void ResumeMatchSave()
    {
        Volatile.Read(ref _saveRelease)?.TrySetResult();
    }

    public void Arm()
    {
        Interlocked.Exchange(ref _remaining, 2);
        Interlocked.Exchange(
            ref _release,
            new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        if (eventData.Context is not MatchmakingWriteDbContext context
            || !context.ChangeTracker.Entries<MatchSearch>()
                .Any(entry => entry.State == EntityState.Modified && entry.Entity.Status == MatchSearchStatus.Matched))
        {
            return result;
        }

        var reached = Interlocked.Exchange(ref _saveReached, null);
        if (reached is not null)
        {
            var saveRelease = Volatile.Read(ref _saveRelease)!;
            reached.TrySetResult();
            await saveRelease.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
        }

        var release = Volatile.Read(ref _release);
        if (release is not null)
        {
            if (Interlocked.Decrement(ref _remaining) == 0)
            {
                Interlocked.Exchange(ref _release, null);
                release.TrySetResult();
            }

            await release.Task.WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
        }

        return result;
    }
}
