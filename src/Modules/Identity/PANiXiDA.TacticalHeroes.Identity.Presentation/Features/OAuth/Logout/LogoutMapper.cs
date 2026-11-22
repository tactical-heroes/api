using PANiXiDA.TacticalHeroes.Modules.Identity.Application.OAuth.Logout;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.OAuth.Logout;

[Mapper]
internal static partial class LogoutMapper
{
    [MapPropertyFromSource(nameof(LogoutCommand.UserId))]
    internal static partial LogoutCommand ToCommand(Guid userId, string authorizationId, string clientId);
}
