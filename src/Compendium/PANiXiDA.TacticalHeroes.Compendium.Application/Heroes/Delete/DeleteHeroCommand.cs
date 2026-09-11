namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Delete;

public sealed record DeleteHeroCommand(Guid Id) : ICommand<Result>;
