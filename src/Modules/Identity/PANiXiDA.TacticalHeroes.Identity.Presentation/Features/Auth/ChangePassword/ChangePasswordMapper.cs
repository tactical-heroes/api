using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ChangePassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ChangePassword;

[Mapper]
internal static partial class ChangePasswordMapper
{
    internal static partial ChangePasswordCommand ToCommand(ChangePasswordRequest request);
}
