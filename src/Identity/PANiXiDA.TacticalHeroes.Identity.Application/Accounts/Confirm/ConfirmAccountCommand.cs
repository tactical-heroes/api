namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Confirm;

public sealed record ConfirmAccountCommand(
    Guid AccountId,
    string EmailConfirmationToken) : ICommand<Result>;
