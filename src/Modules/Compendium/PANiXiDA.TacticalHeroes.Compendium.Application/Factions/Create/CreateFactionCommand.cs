namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Create;

public sealed record CreateFactionCommand(
    string Name,
    string Description) : ICommand<Result<Guid>>;
