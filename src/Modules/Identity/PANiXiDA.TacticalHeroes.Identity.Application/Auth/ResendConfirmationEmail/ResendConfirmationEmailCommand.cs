namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : ICommand<Result>;
