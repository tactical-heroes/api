using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

using Wolverine;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;

public sealed class MatchmakingWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<MatchmakingProcessingOptions> options,
    TimeProvider timeProvider,
    ILogger<MatchmakingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processing = options.Value;
        if (!processing.Enabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(processing.Interval, timeProvider);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var result = await scope.ServiceProvider.GetRequiredService<IMessageBus>()
                    .InvokeAsync<Result>(new ProcessMatchmakingCommand(), stoppingToken);
                if (result.IsFailure)
                {
                    logger.LogWarning("Matchmaking pass failed: {Errors}", result.Errors);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Matchmaking pass failed; the next tick will retry");
            }
        }
    }
}
