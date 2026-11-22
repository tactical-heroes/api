using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Create;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Create;

[Mapper]
internal static partial class CreateUnitMapper
{
    internal static partial CreateUnitCommand ToCommand(
        CreateUnitRequest request);

    internal static partial CreateUnitResponse ToResponse(Guid id);
}
