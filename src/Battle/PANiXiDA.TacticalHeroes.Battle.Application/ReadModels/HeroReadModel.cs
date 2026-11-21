namespace PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;

public sealed record HeroReadModel(
    Guid Id,
    string Name,
    string Description,
    int Attack,
    int Defense,
    int MinimumDamage,
    int MaximumDamage,
    double Initiative,
    int Morale,
    int Luck,
    Guid FactionId) : IReadModel;
