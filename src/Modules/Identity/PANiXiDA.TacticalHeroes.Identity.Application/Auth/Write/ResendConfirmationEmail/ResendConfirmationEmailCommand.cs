namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : ICommand<Result>;
