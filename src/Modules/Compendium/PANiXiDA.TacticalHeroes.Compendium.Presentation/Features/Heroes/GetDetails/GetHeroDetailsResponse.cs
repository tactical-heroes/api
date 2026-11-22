namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetDetails;

public sealed record GetHeroDetailsResponse(
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
