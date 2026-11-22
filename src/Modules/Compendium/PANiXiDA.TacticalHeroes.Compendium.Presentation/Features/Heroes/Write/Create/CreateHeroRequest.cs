namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Write.Create;

public sealed record CreateHeroRequest(
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
