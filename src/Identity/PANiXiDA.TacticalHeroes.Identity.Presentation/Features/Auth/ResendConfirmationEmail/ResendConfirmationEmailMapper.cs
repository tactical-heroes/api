using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

[Mapper]
internal static partial class ResendConfirmationEmailMapper
{
    internal static partial ResendUserConfirmationEmailCommand ToCommand(
        ResendConfirmationEmailRequest request);
}
