using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken;
using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.GetClientTokenPrincipal;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Write.ExchangeToken;

[Mapper]
internal static partial class ExchangeTokenMapper
{
    internal static partial ExchangeTokenQuery ToUserQuery(Guid userId);

    internal static partial GetClientTokenPrincipalQuery ToClientQuery(
        string clientId);
}
