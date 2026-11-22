using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Write.Delete;

[Mapper]
internal static partial class DeleteHeroMapper
{
    internal static partial DeleteHeroCommand ToCommand(Guid id);
}
