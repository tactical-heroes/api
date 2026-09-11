namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

public sealed record GetUnitDetailsQuery(Guid Id)
    : IQuery<Result<UnitDetailsReadModel>>;
