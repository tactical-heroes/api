using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Read.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Read.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetRoleDetailsMapper
{
    internal static partial GetRoleDetailsQuery ToQuery(Guid id);

    internal static partial GetRoleDetailsResponse ToResponse(RoleDetailsReadModel role);
}
