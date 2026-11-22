using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetDetails;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetInternalUnit;

public sealed record GetInternalUnitQuery(Guid Id) : IQuery<Result<UnitDetailsReadModel>>;
