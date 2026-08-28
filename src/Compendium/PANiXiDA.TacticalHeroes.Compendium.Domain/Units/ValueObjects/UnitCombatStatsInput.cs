namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed record UnitCombatStatsInput
{
    public required int Attack { get; init; }
    public required int Defense { get; init; }
    public required int Health { get; init; }
    public required int MinimumDamage { get; init; }
    public required int MaximumDamage { get; init; }
    public required double Initiative { get; init; }
    public required int Speed { get; init; }
    public int? Shots { get; init; }
    public int? RangedAttackRange { get; init; }
}
