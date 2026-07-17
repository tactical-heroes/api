using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.Update;

public sealed record UpdateAccountCommand(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    IReadOnlyCollection<Claim> Claims,
    string Status) : ICommand<Result>;
