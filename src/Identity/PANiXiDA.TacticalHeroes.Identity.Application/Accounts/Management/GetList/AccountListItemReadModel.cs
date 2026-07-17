namespace PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Management.GetList;

public sealed record AccountListItemReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
