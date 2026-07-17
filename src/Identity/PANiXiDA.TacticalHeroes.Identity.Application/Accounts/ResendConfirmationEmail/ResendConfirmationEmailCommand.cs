namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : ICommand<Result>;
