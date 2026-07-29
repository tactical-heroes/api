namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

public sealed record ForgotUserPasswordCommand(string Email) : ICommand<Result>;
