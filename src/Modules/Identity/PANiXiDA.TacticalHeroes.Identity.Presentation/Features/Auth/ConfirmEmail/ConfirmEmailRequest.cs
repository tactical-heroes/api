namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string EmailConfirmationToken);
