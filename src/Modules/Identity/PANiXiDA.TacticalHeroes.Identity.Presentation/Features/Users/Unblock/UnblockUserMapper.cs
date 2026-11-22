using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Unblock;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Unblock;

[Mapper]
internal static partial class UnblockUserMapper
{
    internal static partial UnblockUserCommand ToCommand(Guid id);
}
