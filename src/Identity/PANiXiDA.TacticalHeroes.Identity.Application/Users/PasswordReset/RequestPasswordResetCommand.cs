namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<Result>;
