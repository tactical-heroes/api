namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

public sealed record UpdateHeroCommand(
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
    Guid FactionId) : ICommand<Result>;
