namespace PANiXiDA.TacticalHeroes.Identity.Application.Users.Read.GetStatuses;

public sealed record UserStatusReadModel(
    int Id,
    string Name,
    string DisplayName) : IReadModel;
