using Riok.Mapperly.Abstractions;

using PANiXiDA.TacticalHeroes.Identity.Application.Users.Create;
using PANiXiDA.TacticalHeroes.Identity.Presentation.Common;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Create;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class CreateUserMapper
{
    internal static partial CreateUserCommand ToCommand(CreateUserRequest request);

    internal static partial CreateUserResponse ToResponse(Guid id);
}
