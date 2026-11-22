namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Heroes.Update;

public sealed record UpdateHeroRequest(
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
