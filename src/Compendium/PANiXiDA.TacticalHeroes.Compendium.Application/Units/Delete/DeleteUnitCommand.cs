namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Delete;

public sealed record DeleteUnitCommand(Guid Id) : ICommand<Result>;
