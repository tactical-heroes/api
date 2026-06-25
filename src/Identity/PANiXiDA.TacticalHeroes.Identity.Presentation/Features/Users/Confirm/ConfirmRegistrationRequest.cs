namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Confirm;

internal sealed record ConfirmRegistrationRequest(
    Guid UserId,
    string ConfirmationToken);
