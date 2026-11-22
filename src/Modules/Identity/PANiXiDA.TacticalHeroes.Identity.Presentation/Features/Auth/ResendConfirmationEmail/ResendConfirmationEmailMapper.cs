using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Auth.ResendConfirmationEmail;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

[Mapper]
internal static partial class ResendConfirmationEmailMapper
{
    internal static partial ResendConfirmationEmailCommand ToCommand(
        ResendConfirmationEmailRequest request);
}
