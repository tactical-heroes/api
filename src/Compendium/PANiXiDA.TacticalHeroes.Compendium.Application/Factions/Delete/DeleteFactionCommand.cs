namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Delete;

public sealed record DeleteFactionCommand(Guid Id) : ICommand<Result>;
