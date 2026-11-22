using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Read.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Read.GetDetails;

[Mapper]
internal static partial class GetUnitDetailsMapper
{
    internal static partial GetUnitDetailsQuery ToQuery(Guid id);

    internal static partial GetUnitDetailsResponse ToResponse(
        UnitDetailsReadModel unit);
}
