namespace PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Write.Delete;

public sealed record DeleteHeroCommand(Guid Id) : ICommand<Result>;
