using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;

public sealed record CreateUserCommand(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status) : ICommand<Result<Guid>>;
