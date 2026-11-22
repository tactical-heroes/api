namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetList;

public sealed record RoleListItemReadModel(
    Guid Id,
    string Name,
    bool IsDefault) : IReadModel;
