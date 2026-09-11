using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

[Mapper]
internal static partial class GetFactionSelectOptionsMapper
{
    [MapPropertyFromSource(nameof(GetFactionSelectOptionsQuery.Filter))]
    internal static partial GetFactionSelectOptionsQuery ToQuery(GetFactionSelectOptionsRequest request);

    internal static partial IReadOnlyList<FactionSelectOptionResponse> ToResponse(
        IReadOnlyList<FactionSelectOptionReadModel> options);

    [MapperIgnoreSource(nameof(GetFactionSelectOptionsRequest.Limit))]
    private static partial FactionsFilter ToFilter(GetFactionSelectOptionsRequest request);
}
