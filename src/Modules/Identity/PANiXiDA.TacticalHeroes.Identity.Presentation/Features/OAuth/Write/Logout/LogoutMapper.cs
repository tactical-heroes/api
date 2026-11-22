using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Write.Logout;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Write.Logout;

[Mapper]
internal static partial class LogoutMapper
{
    [MapPropertyFromSource(nameof(LogoutCommand.UserId))]
    internal static partial LogoutCommand ToCommand(Guid userId, string authorizationId, string clientId);
}
