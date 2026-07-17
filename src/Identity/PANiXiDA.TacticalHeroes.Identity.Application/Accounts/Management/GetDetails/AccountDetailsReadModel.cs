using System.Security.Claims;

namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetDetails;

public sealed record AccountDetailsReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName,
    IReadOnlyCollection<Claim> Claims);
