namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
