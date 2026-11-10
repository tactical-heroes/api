namespace PANiXiDA.TacticalHeroes.Compendium.Contracts.Units;

public sealed record UnitResponse(
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
    Guid FactionId);
