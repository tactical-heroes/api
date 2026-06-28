namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetAuthenticated;

public sealed record AuthenticatedUserReadModel(
    Guid Id,
    string Email,
    bool ConfirmationStatus);
