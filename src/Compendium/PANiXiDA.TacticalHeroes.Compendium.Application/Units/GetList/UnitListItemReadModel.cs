namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

public sealed record UnitListItemReadModel(
    Guid Id,
    string Name,
    string Description,
    int Attack,
    int Defense,
    int Health,
    int MinimumDamage,
    int MaximumDamage,
    double Initiative,
    int Speed,
    int? Shots,
    int? RangedAttackRange,
    int Morale,
    int Luck,
    Guid FactionId) : ReadModel;
