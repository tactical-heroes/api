using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.ChangePassword;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ChangePassword;

[Mapper]
internal static partial class ChangePasswordMapper
{
    internal static partial ChangeUserPasswordCommand ToCommand(
        ChangePasswordRequest request,
        Guid userId);
}
