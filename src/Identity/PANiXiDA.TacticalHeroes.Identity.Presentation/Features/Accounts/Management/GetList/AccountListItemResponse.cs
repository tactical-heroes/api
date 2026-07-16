namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetList;

public sealed record AccountListItemResponse(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
