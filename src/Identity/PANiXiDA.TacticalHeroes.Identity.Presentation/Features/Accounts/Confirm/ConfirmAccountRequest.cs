namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Confirm;

public sealed record ConfirmAccountRequest(
    Guid UserId,
    string EmailConfirmationToken);
