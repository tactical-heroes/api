using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Accounts.Register;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Register;

[Mapper]
internal static partial class RegisterAccountMapper
{
    internal static partial RegisterAccountCommand ToCommand(RegisterAccountRequest request);

    internal static partial RegisterAccountResponse ToResponse(Guid id);
}
