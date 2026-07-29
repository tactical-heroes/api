namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ConfirmEmail;

public sealed record ConfirmUserEmailCommand(
    Guid UserId,
    string EmailConfirmationToken) : ICommand<Result>;
