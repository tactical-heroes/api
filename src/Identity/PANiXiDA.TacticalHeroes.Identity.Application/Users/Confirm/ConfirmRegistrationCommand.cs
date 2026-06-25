namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Confirm;

public sealed record ConfirmRegistrationCommand(
    Guid UserId,
    string ConfirmationToken) : ICommand<Result>;
