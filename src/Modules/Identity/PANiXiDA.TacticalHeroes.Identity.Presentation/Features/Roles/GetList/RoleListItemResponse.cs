namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.GetList;

public sealed record RoleListItemResponse(
    Guid Id,
    string Name,
    bool IsDefault);
