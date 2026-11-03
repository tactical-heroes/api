using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Streaming;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchStatePublisherTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "State stream should notify after commit when transaction succeeds")]
    public async Task PublishAsync_Should_NotifyAfterCommit_When_TransactionSucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        var connections = new MatchSearchConnections();
        using var listener = new MatchSearchStateListener(
            fixture.ConnectionString, connections, NullLogger<MatchSearchStateListener>.Instance);
        await listener.StartAsync(ct);
        var rolledBack = connections.Subscribe(Guid.NewGuid());
        var committed = connections.Subscribe(Guid.NewGuid());
        bool delivered;
        try
        {
            await using var scope = fixture.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
            var publisher = new MatchSearchStatePublisher(context);
            await using (var transaction = await context.Database.BeginTransactionAsync(ct))
            {
                await publisher.PublishAsync(rolledBack.PlayerId, ct);
                rolledBack.Changes.Reader.TryRead(out _).ShouldBeFalse();
                await transaction.RollbackAsync(ct);
            }

            await using (var transaction = await context.Database.BeginTransactionAsync(ct))
            {
                await publisher.PublishAsync(committed.PlayerId, ct);
                committed.Changes.Reader.TryRead(out _).ShouldBeFalse();
                await transaction.CommitAsync(ct);
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            delivered = await committed.Changes.Reader.ReadAsync(timeout.Token);
        }
        finally
        {
            await listener.StopAsync(ct);
        }

        delivered.ShouldBeTrue();
        rolledBack.Changes.Reader.TryRead(out _).ShouldBeFalse();
    }
}
