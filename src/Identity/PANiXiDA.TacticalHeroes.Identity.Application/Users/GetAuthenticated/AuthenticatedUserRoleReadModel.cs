namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserRoleReadModel(
    string Name,
    IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims);
