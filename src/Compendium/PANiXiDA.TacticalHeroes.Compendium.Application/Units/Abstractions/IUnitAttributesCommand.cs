namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Abstractions;

public interface IUnitAttributesCommand
{
    string Name { get; }
    string Description { get; }
    int Attack { get; }
    int Defense { get; }
    int Health { get; }
    int MinimumDamage { get; }
    int MaximumDamage { get; }
    double Initiative { get; }
    int Speed { get; }
    int? Shots { get; }
    int? RangedAttackRange { get; }
    int Morale { get; }
    int Luck { get; }
    Guid FactionId { get; }
}
