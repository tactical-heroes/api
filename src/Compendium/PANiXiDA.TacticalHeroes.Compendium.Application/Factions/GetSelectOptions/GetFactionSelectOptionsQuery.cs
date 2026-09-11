using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed record GetFactionSelectOptionsQuery(FactionsFilter Filter, int Limit)
    : IQuery<Result<IReadOnlyList<FactionSelectOptionReadModel>>>;
