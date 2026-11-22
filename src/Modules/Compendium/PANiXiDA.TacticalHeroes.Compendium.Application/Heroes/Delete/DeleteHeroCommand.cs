namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.Delete;

public sealed record DeleteHeroCommand(Guid Id) : ICommand<Result>;
