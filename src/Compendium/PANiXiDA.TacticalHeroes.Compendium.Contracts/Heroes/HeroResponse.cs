namespace PANiXiDA.TacticalHeroes.Compendium.Contracts.Heroes;

public sealed record HeroResponse(
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
    Guid FactionId);
