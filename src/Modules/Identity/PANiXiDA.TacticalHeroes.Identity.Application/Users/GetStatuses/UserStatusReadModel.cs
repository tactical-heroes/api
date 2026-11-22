namespace PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.GetStatuses;

public sealed record UserStatusReadModel(
    int Id,
    string Name,
    string DisplayName) : IReadModel;
