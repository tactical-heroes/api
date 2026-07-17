using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ResetPassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ResetPassword;

[Mapper]
internal static partial class ResetPasswordMapper
{
    [MapProperty(nameof(ResetPasswordRequest.UserId), nameof(ResetPasswordCommand.AccountId))]
    internal static partial ResetPasswordCommand ToCommand(ResetPasswordRequest request);
}
