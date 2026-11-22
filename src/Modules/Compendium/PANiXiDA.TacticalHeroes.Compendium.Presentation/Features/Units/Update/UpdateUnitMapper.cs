using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Units.Update;

[Mapper]
internal static partial class UpdateUnitMapper
{
    internal static partial UpdateUnitCommand ToCommand(
        UpdateUnitRequest request,
        Guid id);
}
