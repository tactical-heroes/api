using PANiXiDA.TacticalHeroes.Identity.Application.Users.GetDetails;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

[Mapper]
internal static partial class AuthorizeMapper
{
    internal static partial GetUserDetailsQuery ToQuery(Guid id);
}
