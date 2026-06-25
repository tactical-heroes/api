namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers;

public sealed record IdentityClaims(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
