namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Read.GetList;

public sealed record RoleListItemResponse(
    Guid Id,
    string Name,
    bool IsDefault);
