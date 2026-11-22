namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Read.GetList;

public sealed record UserListItemReadModel(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName) : IReadModel;
