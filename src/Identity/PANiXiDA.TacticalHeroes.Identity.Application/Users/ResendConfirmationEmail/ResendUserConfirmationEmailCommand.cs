namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.ResendConfirmationEmail;

public sealed record ResendUserConfirmationEmailCommand(string Email) : ICommand<Result>;
