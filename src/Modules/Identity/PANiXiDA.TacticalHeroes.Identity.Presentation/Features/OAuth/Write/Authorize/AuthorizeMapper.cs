using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.Authorize;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Write.Authorize;

[Mapper]
internal static partial class AuthorizeMapper
{
    internal static partial AuthorizeQuery ToQuery(Guid userId);
}
