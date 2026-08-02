using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResetPassword;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    internal static partial ResetPasswordCommand ToCommand(ResetPasswordRequest request);
}
