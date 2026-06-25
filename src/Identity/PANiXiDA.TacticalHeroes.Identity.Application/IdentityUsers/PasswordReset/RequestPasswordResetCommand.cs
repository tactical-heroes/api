namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.PasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<Result>;
