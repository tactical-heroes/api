namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Abstractions;

public sealed record UserGeneratedToken(
    string Value,
    DateTimeOffset ExpiresAtUtc);
