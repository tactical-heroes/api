using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Block;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Block;

[Mapper]
internal static partial class BlockUserMapper
{
    internal static partial BlockUserCommand ToCommand(Guid id);
}
