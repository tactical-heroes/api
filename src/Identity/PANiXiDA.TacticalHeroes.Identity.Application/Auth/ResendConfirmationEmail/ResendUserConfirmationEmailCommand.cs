namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

public sealed record ResendUserConfirmationEmailCommand(string Email) : ICommand<Result>;
