using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.Filters;
using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

[Mapper]
internal static partial class GetFactionSelectOptionsMapper
{
    internal static GetFactionSelectOptionsQuery ToQuery(GetFactionSelectOptionsRequest request)
    {
        return new GetFactionSelectOptionsQuery(
            new FactionsFilter(request.Search),
            request.Limit);
    }

    internal static partial IReadOnlyList<FactionSelectOptionResponse> ToResponse(
        IReadOnlyList<FactionSelectOptionReadModel> options);
}
