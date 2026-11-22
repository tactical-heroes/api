using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Handlers;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Identity.IntegrationTests.Infrastructure.Messaging;

public sealed class UserRegisteredHandlerTests
{
    [Fact(DisplayName = "Registration mapping should preserve identity and timestamp when user is registered")]
    public void Handle_Should_PreserveIdentityAndTimestamp_When_UserIsRegistered()
    {
        var domainEvent = new DomainEvent(Guid.CreateVersion7(), "hero@example.com");

        var integrationEvent = UserRegisteredHandler.Handle(domainEvent);

        integrationEvent.UserId.ShouldBe(domainEvent.UserId);
        integrationEvent.Id.ShouldBe(domainEvent.Id);
        integrationEvent.OccurredOnUtc.ShouldBe(domainEvent.OccurredOnUtc);
    }
}
