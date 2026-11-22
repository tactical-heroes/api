using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;

public sealed record GetFactionSelectOptionsQuery(FactionsFilter Filter, LimitParameters Limit)
    : IQuery<Result<IReadOnlyList<FactionSelectOptionReadModel>>>;
