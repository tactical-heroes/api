using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.GetDetails;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class GetUserDetailsMapper
{
    [MapperIgnoreSource(nameof(UserDetailsReadModel.IsBlocked))]
    internal static partial GetUserDetailsResponse ToResponse(UserDetailsReadModel user);
}
