namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Update;

public sealed record UpdateFactionCommand(
    Guid Id,
    string Name,
    string Description) : ICommand<Result>;
