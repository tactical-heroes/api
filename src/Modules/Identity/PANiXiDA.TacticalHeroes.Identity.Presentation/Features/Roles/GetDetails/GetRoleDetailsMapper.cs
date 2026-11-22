using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetRoleDetailsMapper
{
    internal static partial GetRoleDetailsQuery ToQuery(Guid id);

    internal static partial GetRoleDetailsResponse ToResponse(RoleDetailsReadModel role);
}
