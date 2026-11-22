using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Update;
using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateRoleMapper
{
    internal static partial UpdateRoleCommand ToCommand(
        UpdateRoleRequest request,
        Guid id);
}
