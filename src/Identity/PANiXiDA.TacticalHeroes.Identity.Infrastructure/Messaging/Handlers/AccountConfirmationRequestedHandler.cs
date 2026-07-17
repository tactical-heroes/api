using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Builders;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.AccountConfirmationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.AccountConfirmationRequested;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Handlers;

public sealed class AccountConfirmationRequestedHandler(
    IOptions<IdentityMessagingOptions> options)
{
    public IntegrationEvent Handle(DomainEvent domainEvent)
    {
        var confirmationUrl = IdentityLinkBuilder.Build(
            template: options.Value.AccountConfirmationUrlTemplate,
            userId: domainEvent.UserId,
            token: domainEvent.ConfirmationToken);

        return new IntegrationEvent(
            UserId: domainEvent.UserId,
            Email: domainEvent.Email,
            ConfirmationUrl: confirmationUrl,
            ExpiresAtUtc: domainEvent.ExpiresAtUtc);
    }
}
