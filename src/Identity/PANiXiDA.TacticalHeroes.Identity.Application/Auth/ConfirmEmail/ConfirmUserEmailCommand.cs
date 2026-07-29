namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ConfirmEmail;

public sealed record ConfirmUserEmailCommand(
    Guid UserId,
    string EmailConfirmationToken) : ICommand<Result>;
