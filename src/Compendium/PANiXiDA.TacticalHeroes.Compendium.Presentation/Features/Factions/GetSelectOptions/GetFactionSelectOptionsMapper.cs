using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

[Mapper]
internal static partial class GetFactionSelectOptionsMapper
{
    [MapPropertyFromSource(nameof(GetFactionSelectOptionsQuery.Filter))]
    internal static partial GetFactionSelectOptionsQuery ToQuery(
        GetFactionSelectOptionsRequest request,
        LimitParameters limit);

    internal static partial IReadOnlyList<FactionSelectOptionResponse> ToResponse(
        IReadOnlyList<FactionSelectOptionReadModel> options);
}
