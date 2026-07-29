namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    Guid UserId,
    string EmailConfirmationToken) : ICommand<Result>;
