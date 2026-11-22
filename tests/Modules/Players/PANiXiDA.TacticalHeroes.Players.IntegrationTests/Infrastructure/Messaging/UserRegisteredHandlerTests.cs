using PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Messaging.Handlers;

using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.UserRegistered;

namespace PANiXiDA.TacticalHeroes.Modules.Players.IntegrationTests.Infrastructure.Messaging;

public sealed class UserRegisteredHandlerTests
{
    [Fact(DisplayName = "Registration mapping should preserve the user when registration is received")]
    public void Handle_Should_PreserveUser_When_RegistrationIsReceived()
    {
        var integrationEvent = new IntegrationEvent(Guid.CreateVersion7());

        var domainEvent = UserRegisteredHandler.Handle(integrationEvent);

        domainEvent.UserId.ShouldBe(integrationEvent.UserId);
    }
}
