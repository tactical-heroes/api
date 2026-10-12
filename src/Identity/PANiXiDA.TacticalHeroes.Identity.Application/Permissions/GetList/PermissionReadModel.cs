namespace PANiXiDA.TacticalHeroes.Identity.Application.Permissions.GetList;

public sealed record PermissionReadModel(
    string Code,
    string DisplayName,
    string Group) : IReadModel;
