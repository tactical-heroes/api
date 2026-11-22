using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Heroes.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Heroes.Update;

[Mapper]
internal static partial class UpdateHeroMapper
{
    internal static partial UpdateHeroCommand ToCommand(
        UpdateHeroRequest request,
        Guid id);
}
