using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Builders;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Options.IdentityMessaging;

using DomainEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Events.EmailConfirmationRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.EmailConfirmationRequested;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Handlers;

public sealed class EmailConfirmationRequestedHandler(
    IOptions<IdentityMessagingOptions> options)
{
    public IntegrationEvent Handle(DomainEvent domainEvent)
    {
        var confirmationUrl = IdentityLinkBuilder.Build(
            template: options.Value.EmailConfirmationUrlTemplate,
            userId: domainEvent.UserId,
            token: domainEvent.ConfirmationToken);

        return new IntegrationEvent(
            UserId: domainEvent.UserId,
            Email: domainEvent.Email,
            ConfirmationUrl: confirmationUrl,
            ExpiresAtUtc: domainEvent.ExpiresAtUtc)
        {
            Id = domainEvent.Id,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }
}
