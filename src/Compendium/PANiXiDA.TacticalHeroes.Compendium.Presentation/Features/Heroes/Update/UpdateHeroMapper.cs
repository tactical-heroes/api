using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Update;

[Mapper]
internal static partial class UpdateHeroMapper
{
    internal static partial UpdateHeroCommand ToCommand(
        UpdateHeroRequest request,
        Guid id);
}
