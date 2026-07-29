namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : ICommand<Result>;
