namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Confirm;

public sealed record ConfirmRegistrationCommand(
    Guid UserId,
    string ConfirmationToken) : ICommand<Result>;
