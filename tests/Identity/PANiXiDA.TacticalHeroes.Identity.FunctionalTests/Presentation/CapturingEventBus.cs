using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Domain.DomainEvents;

using System.Collections.Concurrent;

namespace PANiXiDA.TacticalHeroes.Identity.FunctionalTests.Presentation;

internal sealed class CapturingEventBus : IEventBus
{
    private readonly ConcurrentQueue<object> _events = new();

    public Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken)
        where TEvent : DomainEvent
    {
        _events.Enqueue(@event!);

        return Task.CompletedTask;
    }

    public void Clear()
    {
        _events.Clear();
    }

    public TEvent Single<TEvent>()
    {
        var events = _events
            .OfType<TEvent>()
            .ToArray();

        events.Length.ShouldBe(1);

        return events[0];
    }
}
