using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users;

public sealed record UserClaims(
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims);
