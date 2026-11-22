namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.Write.Delete;

public sealed record DeleteUnitCommand(Guid Id) : ICommand<Result>;
