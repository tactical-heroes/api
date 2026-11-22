namespace PANiXiDA.TacticalHeroes.Identity.Application.Auth.Write.Register;

public sealed record RegisterUserCommand(
    string Email,
    string UserName,
    string Password) : ICommand<Result<Guid>>;
