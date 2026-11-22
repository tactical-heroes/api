namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Read.GetList;

public sealed record UserListItemResponse(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
