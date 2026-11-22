using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Write.Update;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Write.Update;

[Mapper]
internal static partial class UpdateHeroMapper
{
    internal static partial UpdateHeroCommand ToCommand(
        UpdateHeroRequest request,
        Guid id);
}
