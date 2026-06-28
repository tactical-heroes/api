namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserReadModel(
    Guid Id,
    string Email,
    bool ConfirmationStatus,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<AuthenticatedUserClaimReadModel> Claims);

public sealed record AuthenticatedUserClaimReadModel(
    string Type,
    string Value);
