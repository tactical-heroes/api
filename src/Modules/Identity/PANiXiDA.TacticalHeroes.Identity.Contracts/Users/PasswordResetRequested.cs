namespace PANiXiDA.TacticalHeroes.Modules.Identity.Contracts.Users;

public sealed record PasswordResetRequested(
    Guid UserId,
    string Email,
    string PasswordResetUrl,
    DateTimeOffset ExpiresAtUtc) : IntegrationEvent;
