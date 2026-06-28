namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserClaimReadModel(
    string Type,
    string Value);
