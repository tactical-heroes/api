using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Write.Update;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Write.Update;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class UpdateRoleMapper
{
    internal static partial UpdateRoleCommand ToCommand(
        UpdateRoleRequest request,
        Guid id);
}
