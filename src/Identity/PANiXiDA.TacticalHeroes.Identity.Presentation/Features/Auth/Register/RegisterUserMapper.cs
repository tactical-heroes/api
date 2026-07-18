using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Auth.Register;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Auth.Register;

[Mapper]
internal static partial class RegisterUserMapper
{
    internal static partial RegisterUserCommand ToCommand(RegisterUserRequest request);

    internal static partial RegisterUserResponse ToResponse(Guid id);
}
