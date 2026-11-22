using PANiXiDA.TacticalHeroes.Identity.Application.Users.Unblock;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Unblock;

[Mapper]
internal static partial class UnblockUserMapper
{
    internal static partial UnblockUserCommand ToCommand(Guid id);
}
