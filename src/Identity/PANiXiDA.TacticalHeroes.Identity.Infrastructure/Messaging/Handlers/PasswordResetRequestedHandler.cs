using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Builders;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.PasswordResetRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.PasswordResetRequested;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Handlers;

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
            ExpiresAtUtc: domainEvent.ExpiresAtUtc);
    }
}
