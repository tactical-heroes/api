namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ConfirmEmail;

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string EmailConfirmationToken);
