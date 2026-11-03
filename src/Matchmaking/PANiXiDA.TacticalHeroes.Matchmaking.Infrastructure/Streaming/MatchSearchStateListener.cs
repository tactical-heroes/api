using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

internal sealed class MatchSearchStateListener(
    string connectionString,
    MatchSearchConnections connections,
    ILogger<MatchSearchStateListener> logger) : BackgroundService
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await base.StartAsync(cancellationToken);
        await _ready.Task.WaitAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = new NpgsqlConnection(new NpgsqlConnectionStringBuilder(connectionString)
                {
                    KeepAlive = 30,
                    Pooling = false
                }.ConnectionString);
                connection.Notification += OnNotification;
                await connection.OpenAsync(stoppingToken);
                await using var command = new NpgsqlCommand($"LISTEN {MatchSearchStatePublisher.Channel}", connection);
                await command.ExecuteNonQueryAsync(stoppingToken);
                connections.RefreshAll();
                _ready.TrySetResult();

                while (!stoppingToken.IsCancellationRequested)
                {
                    await connection.WaitAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (NpgsqlException exception)
            {
                if (!_ready.Task.IsCompleted)
                {
                    _ready.TrySetException(exception);
                    throw;
                }

                logger.LogWarning(exception, "Match search notifications disconnected; reconnecting.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }
    }

    private void OnNotification(object sender, NpgsqlNotificationEventArgs notification)
    {
        if (Guid.TryParse(notification.Payload, out var playerId))
        {
            connections.Refresh(playerId);
        }
    }
}
