using PANiXiDA.Core.Application.Querying.Limiting;
using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Factions.GetSelectOptions;

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
