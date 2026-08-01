namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Create;

public sealed record CreateUnitRequest(
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
