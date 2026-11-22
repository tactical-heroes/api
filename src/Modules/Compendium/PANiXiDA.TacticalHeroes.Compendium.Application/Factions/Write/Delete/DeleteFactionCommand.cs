namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Write.Delete;

public sealed record DeleteFactionCommand(Guid Id) : ICommand<Result>;
