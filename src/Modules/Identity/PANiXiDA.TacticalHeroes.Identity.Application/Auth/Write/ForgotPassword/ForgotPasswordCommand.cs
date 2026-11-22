namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
