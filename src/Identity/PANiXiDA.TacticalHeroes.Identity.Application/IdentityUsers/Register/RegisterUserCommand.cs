namespace PANiXiDA.TacticalHeroes.Identity.Application.IdentityUsers.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password) : ICommand<Result<RegisterUserResult>>;
