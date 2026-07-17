using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Confirm;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Confirm;

[Mapper]
internal static partial class ConfirmAccountMapper
{
    internal static partial ConfirmAccountCommand ToCommand(ConfirmAccountRequest request);
}
