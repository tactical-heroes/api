namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.PasswordReset.Request;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<Result>;
