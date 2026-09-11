using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetSelectOptions;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetSelectOptions;

[Mapper]
internal static partial class GetFactionSelectOptionsMapper
{
    internal static partial GetFactionSelectOptionsQuery ToQuery(GetFactionSelectOptionsRequest request);

    internal static partial IReadOnlyList<FactionSelectOptionResponse> ToResponse(
        IReadOnlyList<FactionSelectOptionReadModel> options);
}
