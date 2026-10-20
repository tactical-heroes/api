using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.ExchangeToken;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Authorize;

[Mapper]
internal static partial class AuthorizeMapper
{
    internal static partial ExchangeTokenQuery ToQuery(Guid userId);
}
