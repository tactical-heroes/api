namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Create;

public sealed record CreateHeroCommand(
    string Name,
    string Description,
    int Attack,
    int Defense,
    int MinimumDamage,
    int MaximumDamage,
    double Initiative,
    int Morale,
    int Luck,
    Guid FactionId) : ICommand<Result<Guid>>;
