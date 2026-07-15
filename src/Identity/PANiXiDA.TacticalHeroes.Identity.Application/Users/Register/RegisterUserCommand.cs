namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Register;

public sealed record RegisterUserCommand(
    string Email,
    string Password) : ICommand<Result<Guid>>;
