using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Create;
using PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Common;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Create;

[Mapper]
[UseStaticMapper(typeof(Claim))]
internal static partial class CreateUserMapper
{
    internal static partial CreateUserCommand ToCommand(CreateUserRequest request);

    internal static partial CreateUserResponse ToResponse(Guid id);
}
