namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ForgotPassword;

public sealed record ForgotUserPasswordCommand(string Email) : ICommand<Result>;
