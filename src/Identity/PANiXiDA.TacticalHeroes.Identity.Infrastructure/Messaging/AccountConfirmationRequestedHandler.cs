using Microsoft.Extensions.Options;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.AccountConfirmationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.AccountConfirmationRequested;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging;

public sealed class AccountConfirmationRequestedHandler(
    IOptions<IdentityMessagingOptions> options)
{
    public IntegrationEvent Handle(DomainEvent domainEvent)
    {
        var confirmationUrl = IdentityLinkBuilder.Build(
            options.Value.AccountConfirmationUrlTemplate,
            domainEvent.UserId,
            domainEvent.ConfirmationToken);

        return new IntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email,
            confirmationUrl,
            domainEvent.ExpiresAtUtc);
    }
}
