using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

public sealed record GetFactionSelectOptionsQuery(FactionsFilter Filter, LimitParameters Limit)
    : IQuery<Result<IReadOnlyList<FactionSelectOptionReadModel>>>;
