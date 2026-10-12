namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Permissions.GetList;

public sealed record PermissionResponse(
    string Code,
    string DisplayName,
    string Group);
