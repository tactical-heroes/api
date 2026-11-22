namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

public sealed record RegisterUserCommand(
    string Email,
    string UserName,
    string Password) : ICommand<Result<Guid>>;
