using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResetPassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    internal static partial ResetPasswordCommand ToCommand(ResetPasswordRequest request);
}
