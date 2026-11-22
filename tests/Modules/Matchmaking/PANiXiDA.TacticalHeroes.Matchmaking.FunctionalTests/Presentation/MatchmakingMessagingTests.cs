using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Infrastructure.Messaging.Wolverine.OutboxDispatcher;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class MatchmakingMessagingTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Host should provide Matchmaking messaging and a scoped outbox when started")]
    public async Task Host_Should_ProvideMatchmakingMessagingAndScopedOutbox_When_Started()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = fixture.Services.CreateAsyncScope();
        await using var otherScope = fixture.Services.CreateAsyncScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var eventBus = scope.ServiceProvider.GetRequiredService<IEventBus>();
        var scheduler = scope.ServiceProvider.GetRequiredService<IScheduler>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext));
        var outbox = scope.ServiceProvider.GetRequiredKeyedService<IOutboxDispatcher>(
            typeof(MatchmakingWriteDbContext));
        var sameScopeOutbox = scope.ServiceProvider.GetRequiredKeyedService<IOutboxDispatcher>(
            typeof(MatchmakingWriteDbContext));
        var otherScopeOutbox = otherScope.ServiceProvider.GetRequiredKeyedService<IOutboxDispatcher>(
            typeof(MatchmakingWriteDbContext));
        await outbox.FlushAsync(cancellationToken);

        mediator.ShouldNotBeNull();
        eventBus.ShouldNotBeNull();
        scheduler.ShouldNotBeNull();
        unitOfWork.HasActiveTransaction.ShouldBeFalse();
        outbox.ShouldBeSameAs(sameScopeOutbox);
        outbox.ShouldNotBeSameAs(otherScopeOutbox);
    }
}
