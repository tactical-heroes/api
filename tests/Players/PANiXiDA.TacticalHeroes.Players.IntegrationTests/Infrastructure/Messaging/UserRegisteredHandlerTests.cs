using PANiXiDA.TacticalHeroes.Players.Infrastructure.Messaging.Handlers;

using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Players.IntegrationTests.Infrastructure.Messaging;

public sealed class UserRegisteredHandlerTests
{
    [Fact(DisplayName = "Registration mapping should preserve user and correlation when registration is received")]
    public void Handle_Should_PreserveUserAndCorrelation_When_RegistrationIsReceived()
    {
        var integrationEvent = new IntegrationEvent(Guid.CreateVersion7());

        var domainEvent = UserRegisteredHandler.Handle(integrationEvent);

        domainEvent.UserId.ShouldBe(integrationEvent.UserId);
        domainEvent.IntegrationEventId.ShouldBe(integrationEvent.Id);
    }
}
