using PANiXiDA.TacticalHeroes.Identity.Application.Auth.ResendConfirmationEmail;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.ResendConfirmationEmail;

[Mapper]
internal static partial class ResendConfirmationEmailMapper
{
    internal static partial ResendConfirmationEmailCommand ToCommand(
        ResendConfirmationEmailRequest request);
}
