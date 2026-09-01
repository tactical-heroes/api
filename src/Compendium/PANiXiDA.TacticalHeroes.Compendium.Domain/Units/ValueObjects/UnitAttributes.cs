namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Units.ValueObjects;

public sealed record UnitAttributes
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required UnitCombatStatsInput CombatStats { get; init; }
    public required int Morale { get; init; }
    public required int Luck { get; init; }
    public required Guid FactionId { get; init; }
}
