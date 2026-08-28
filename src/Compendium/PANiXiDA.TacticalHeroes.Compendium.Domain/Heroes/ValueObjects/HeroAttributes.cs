namespace PANiXiDA.TacticalHeroes.Compendium.Domain.Heroes.ValueObjects;

public sealed record HeroAttributes
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required int Attack { get; init; }
    public required int Defense { get; init; }
    public required int MinimumDamage { get; init; }
    public required int MaximumDamage { get; init; }
    public required double Initiative { get; init; }
    public required int Morale { get; init; }
    public required int Luck { get; init; }
    public required Guid FactionId { get; init; }
}
