using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Options;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Services;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.PasswordResetRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.PasswordResetRequested;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging.Handlers;

public sealed class PasswordResetRequestedHandler(
    IOptions<IdentityMessagingOptions> options)
{
    public IntegrationEvent Handle(DomainEvent domainEvent)
    {
        var passwordResetUrl = IdentityLinkBuilder.Build(
            options.Value.PasswordResetUrlTemplate,
            domainEvent.UserId,
            domainEvent.PasswordResetToken);

        return new IntegrationEvent(
            domainEvent.UserId,
            domainEvent.Email,
            passwordResetUrl,
            domainEvent.ExpiresAtUtc);
    }
}
