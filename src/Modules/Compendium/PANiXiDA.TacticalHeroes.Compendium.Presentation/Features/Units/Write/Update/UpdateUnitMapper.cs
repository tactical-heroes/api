using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Write.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Write.Update;

[Mapper]
internal static partial class UpdateUnitMapper
{
    internal static partial UpdateUnitCommand ToCommand(
        UpdateUnitRequest request,
        Guid id);
}
