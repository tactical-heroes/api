using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

public sealed record CreateUnitCommand(
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
    Guid FactionId) : ICommand<Result<Guid>>, IUnitAttributesCommand;
