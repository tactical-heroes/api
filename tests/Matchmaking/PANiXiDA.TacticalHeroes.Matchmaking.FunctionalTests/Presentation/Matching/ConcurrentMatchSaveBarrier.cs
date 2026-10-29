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
    private long _queueDelayTicks;
    private int _activeQueueSaves;
    private int _maximumQueueSaves;
    private int _failedQueueSaves;

    public int MaximumConcurrentQueueSaves => Volatile.Read(ref _maximumQueueSaves);
    public int FailedQueueSaves => Volatile.Read(ref _failedQueueSaves);

    public void DelayAndFailNextQueueSave()
    {
        Interlocked.Exchange(ref _queueDelayTicks, TimeSpan.FromMilliseconds(2200).Ticks);
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
        if (eventData.Context is MatchmakingWriteDbContext queueContext
            && !queueContext.ChangeTracker.Entries<MatchSearch>().Any())
        {
            var active = Interlocked.Increment(ref _activeQueueSaves);
            var observed = Volatile.Read(ref _maximumQueueSaves);
            while (active > observed)
            {
                var previous = Interlocked.CompareExchange(ref _maximumQueueSaves, active, observed);
                if (previous == observed)
                {
                    break;
                }

                observed = previous;
            }

            try
            {
                var delayTicks = Interlocked.Exchange(ref _queueDelayTicks, 0);
                if (delayTicks > 0)
                {
                    await Task.Delay(TimeSpan.FromTicks(delayTicks), cancellationToken);
                    Interlocked.Increment(ref _failedQueueSaves);
                    throw new InvalidOperationException("Injected matchmaking queue save failure.");
                }
            }
            finally
            {
                Interlocked.Decrement(ref _activeQueueSaves);
            }
        }

        var release = Volatile.Read(ref _release);
        if (release is not null
            && eventData.Context is MatchmakingWriteDbContext context
            && context.ChangeTracker.Entries<MatchSearch>()
                .Any(entry => entry.State == EntityState.Modified && entry.Entity.Status == MatchSearchStatus.Matched))
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
