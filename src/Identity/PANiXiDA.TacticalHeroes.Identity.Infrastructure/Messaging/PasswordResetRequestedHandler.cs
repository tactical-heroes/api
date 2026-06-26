using Microsoft.Extensions.Options;

using DomainEvent = PANiXiDA.TacticalHeroes.Identity.Domain.Users.Events.PasswordResetRequested;
using IntegrationEvent = PANiXiDA.TacticalHeroes.Identity.Contracts.Users.PasswordResetRequested;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Messaging;

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
