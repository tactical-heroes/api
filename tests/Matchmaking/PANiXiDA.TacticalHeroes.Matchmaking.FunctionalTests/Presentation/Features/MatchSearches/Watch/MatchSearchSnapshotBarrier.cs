using System.Data.Common;

using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Features.MatchSearches.Watch;

internal sealed class MatchSearchSnapshotBarrier : DbCommandInterceptor
{
    private int _armed;
    private TaskCompletionSource _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    internal Task Reached => _reached.Task;

    public override async ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken)
    {
        if (command.CommandText.Contains("match_searches", StringComparison.Ordinal)
            && Interlocked.CompareExchange(ref _armed, 0, 1) == 1)
        {
            _reached.TrySetResult();
            await _release.Task.WaitAsync(cancellationToken);
        }

        return result;
    }

    internal void Arm()
    {
        _reached = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Interlocked.Exchange(ref _armed, 1);
    }

    internal void Release() => _release.TrySetResult();
}
