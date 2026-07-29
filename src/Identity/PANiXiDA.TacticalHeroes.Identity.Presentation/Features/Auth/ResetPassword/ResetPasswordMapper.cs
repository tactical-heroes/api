using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    internal static partial ResetUserPasswordCommand ToCommand(ResetPasswordRequest request);
}
