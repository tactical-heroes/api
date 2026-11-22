namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetDetails;

public sealed record HeroDetailsReadModel(
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
    Guid FactionId) : IReadModel;
