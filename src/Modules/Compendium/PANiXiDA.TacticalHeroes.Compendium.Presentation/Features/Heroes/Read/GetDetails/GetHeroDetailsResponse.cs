namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Read.GetDetails;

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
