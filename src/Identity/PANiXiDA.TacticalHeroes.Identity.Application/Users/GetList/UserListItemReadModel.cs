namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.GetList;

public sealed record UserListItemReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
