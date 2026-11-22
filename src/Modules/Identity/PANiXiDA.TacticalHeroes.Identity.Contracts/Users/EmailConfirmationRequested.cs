namespace PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users;

public sealed record EmailConfirmationRequested(
    Guid UserId,
    string Email,
    string ConfirmationUrl,
    DateTimeOffset ExpiresAtUtc) : IntegrationEvent;
