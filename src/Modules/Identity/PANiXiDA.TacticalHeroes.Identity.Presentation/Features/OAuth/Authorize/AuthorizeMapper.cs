using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Authorize;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.Authorize;

[Mapper]
internal static partial class AuthorizeMapper
{
    internal static partial AuthorizeQuery ToQuery(Guid userId);
}
