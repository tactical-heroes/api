namespace PANiXiDA.TacticalHeroes.Identity.Contracts.Users;

public sealed record AccountConfirmationRequested(
    Guid UserId,
    string Email,
    string ConfirmationUrl,
    DateTimeOffset ExpiresAtUtc) : IntegrationEvent;
