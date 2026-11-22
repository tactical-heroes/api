namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ConfirmEmail;

public sealed record ConfirmEmailCommand(
    Guid UserId,
    string EmailConfirmationToken) : ICommand<Result>;
