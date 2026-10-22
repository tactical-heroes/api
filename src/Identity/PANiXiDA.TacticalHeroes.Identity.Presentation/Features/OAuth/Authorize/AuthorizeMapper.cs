using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Authorize;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

[Mapper]
internal static partial class AuthorizeMapper
{
    internal static partial AuthorizeQuery ToQuery(Guid userId);
}
