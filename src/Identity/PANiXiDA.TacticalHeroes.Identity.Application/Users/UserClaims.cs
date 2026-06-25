namespace PANiXiDA.TacticalHeroes.Identity.Application.Users;

public sealed record UserClaims(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<AuthorizationClaim> Claims);
