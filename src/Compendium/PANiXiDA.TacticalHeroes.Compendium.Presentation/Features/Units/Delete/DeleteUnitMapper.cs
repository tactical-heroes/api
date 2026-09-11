using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Delete;

[Mapper]
internal static partial class DeleteUnitMapper
{
    internal static partial DeleteUnitCommand ToCommand(Guid id);
}
