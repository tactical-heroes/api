namespace PANiXiDA.TacticalHeroes.Identity.Contracts.Users;

public sealed record PasswordResetRequested(
    Guid UserId,
    string Email,
    string PasswordResetUrl,
    DateTimeOffset ExpiresAtUtc) : IntegrationEvent;
