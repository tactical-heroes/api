using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Common.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

[Mapper]
internal static partial class GetFactionSelectOptionsMapper
{
    [MapPropertyFromSource(nameof(GetFactionSelectOptionsQuery.Filter))]
    [MapPropertyFromSource(nameof(GetFactionSelectOptionsQuery.Limit))]
    internal static partial GetFactionSelectOptionsQuery ToQuery(GetFactionSelectOptionsRequest request);

    internal static partial IReadOnlyList<FactionSelectOptionResponse> ToResponse(
        IReadOnlyList<FactionSelectOptionReadModel> options);

    [MapperIgnoreSource(nameof(GetFactionSelectOptionsRequest.Limit))]
    private static partial FactionsFilter ToFilter(GetFactionSelectOptionsRequest request);

    [MapperIgnoreSource(nameof(GetFactionSelectOptionsRequest.Search))]
    private static partial LimitParameters ToLimit(GetFactionSelectOptionsRequest request);
}
