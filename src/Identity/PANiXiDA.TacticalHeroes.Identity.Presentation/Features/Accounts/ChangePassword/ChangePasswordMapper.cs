using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.ChangePassword;

[Mapper]
internal static partial class ChangePasswordMapper
{
    internal static partial ChangePasswordCommand ToCommand(
        ChangePasswordRequest request,
        Guid accountId);
}
