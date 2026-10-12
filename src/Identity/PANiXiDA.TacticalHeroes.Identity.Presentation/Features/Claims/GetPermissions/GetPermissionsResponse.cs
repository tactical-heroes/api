namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetPermissions;

public sealed record GetPermissionsResponse(string Type, IReadOnlyCollection<PermissionResponse> Items);
