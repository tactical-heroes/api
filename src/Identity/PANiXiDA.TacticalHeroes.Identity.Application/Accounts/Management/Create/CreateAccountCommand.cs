using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Create;

public sealed record CreateAccountCommand(
    string Email,
    string UserName,
    string Password,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status) : ICommand<Result<Guid>>;
