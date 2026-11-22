using PANiXiDA.TacticalHeroes.Compendium.Application.Units.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.Write.Delete;

[Mapper]
internal static partial class DeleteUnitMapper
{
    internal static partial DeleteUnitCommand ToCommand(Guid id);
}
