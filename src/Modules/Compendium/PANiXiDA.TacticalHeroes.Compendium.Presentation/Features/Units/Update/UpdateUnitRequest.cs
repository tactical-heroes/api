namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Units.Update;

public sealed record UpdateUnitRequest(
    string Name,
    string Description,
    int Attack,
    int Defense,
    int Health,
    int MinimumDamage,
    int MaximumDamage,
    double Initiative,
    int Speed,
    int Morale,
    int Luck,
    Guid FactionId,
    int? Shots = null,
    int? RangedAttackRange = null);
