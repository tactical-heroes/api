using PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Logout;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth.Logout;

[Mapper]
internal static partial class LogoutMapper
{
    [MapPropertyFromSource(nameof(LogoutCommand.UserId))]
    internal static partial LogoutCommand ToCommand(Guid userId, string authorizationId, string clientId);
}
