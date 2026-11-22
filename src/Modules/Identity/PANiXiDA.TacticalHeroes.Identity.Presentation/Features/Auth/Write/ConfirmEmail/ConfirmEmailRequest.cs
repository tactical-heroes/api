namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Write.ConfirmEmail;

public sealed record ConfirmEmailRequest(
    Guid UserId,
    string EmailConfirmationToken);
