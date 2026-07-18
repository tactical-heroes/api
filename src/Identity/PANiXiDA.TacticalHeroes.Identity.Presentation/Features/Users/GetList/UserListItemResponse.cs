namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetList;

public sealed record UserListItemResponse(
    Guid Id,
    string Email,
    string UserName,
    bool IsConfirmed,
    string Status,
    string StatusDisplayName);
