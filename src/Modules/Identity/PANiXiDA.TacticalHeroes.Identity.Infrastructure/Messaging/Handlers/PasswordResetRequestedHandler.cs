using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Builders;
using PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Options.IdentityMessaging;

using DomainEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Domain.Users.Events.PasswordResetRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users.PasswordResetRequested;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Infrastructure.Messaging.Handlers;

public sealed class PasswordResetRequestedHandler(
    IOptions<IdentityMessagingOptions> options)
{
    public IntegrationEvent Handle(DomainEvent domainEvent)
    {
        var passwordResetUrl = IdentityLinkBuilder.Build(
            template: options.Value.PasswordResetUrlTemplate,
            userId: domainEvent.UserId,
            token: domainEvent.PasswordResetToken);

        return new IntegrationEvent(
            UserId: domainEvent.UserId,
            Email: domainEvent.Email,
            PasswordResetUrl: passwordResetUrl,
            ExpiresAtUtc: domainEvent.ExpiresAtUtc)
        {
            Id = domainEvent.Id,
            OccurredOnUtc = domainEvent.OccurredOnUtc
        };
    }
}
