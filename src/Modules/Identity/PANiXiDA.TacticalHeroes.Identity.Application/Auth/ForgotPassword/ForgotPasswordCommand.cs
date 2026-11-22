namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
