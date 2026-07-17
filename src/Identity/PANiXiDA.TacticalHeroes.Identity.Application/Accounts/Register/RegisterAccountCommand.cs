namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Register;

public sealed record RegisterAccountCommand(
    string Email,
    string UserName,
    string Password) : ICommand<Result<Guid>>;
