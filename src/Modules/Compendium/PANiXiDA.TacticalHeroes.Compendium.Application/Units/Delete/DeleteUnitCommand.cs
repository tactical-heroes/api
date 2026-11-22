namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.Delete;

public sealed record DeleteUnitCommand(Guid Id) : ICommand<Result>;
