using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Units.GetDetails;

[Mapper]
internal static partial class GetUnitDetailsMapper
{
    internal static partial GetUnitDetailsQuery ToQuery(Guid id);

    internal static partial GetUnitDetailsResponse ToResponse(
        UnitDetailsReadModel unit);
}
