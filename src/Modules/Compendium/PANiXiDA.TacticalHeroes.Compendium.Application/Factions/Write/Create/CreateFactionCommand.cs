namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Write.Create;

public sealed record CreateFactionCommand(
    string Name,
    string Description) : ICommand<Result<Guid>>;
