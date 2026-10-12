namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Claims.GetPermissions;

public sealed record GetPermissionsResponse(string Type, IReadOnlyCollection<PermissionResponse> Items);

public sealed record PermissionResponse(
    string Code,
    string DisplayName,
    string SectionCode,
    string ResourceCode,
    string ResourceDisplayName);
