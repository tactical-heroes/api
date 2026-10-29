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
