namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.IdentityUsers.Confirm;

internal sealed record ConfirmRegistrationRequest(
    Guid UserId,
    string ConfirmationToken);
