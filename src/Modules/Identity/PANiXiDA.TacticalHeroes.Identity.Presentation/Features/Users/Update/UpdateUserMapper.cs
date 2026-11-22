using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Update;
using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateUserMapper
{
    internal static partial UpdateUserCommand ToCommand(
        UpdateUserRequest request,
        Guid id);
}
