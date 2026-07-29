using PANiXiDA.TacticalHeroes.Identity.Application.Users.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Delete;

[Mapper]
internal static partial class DeleteUserMapper
{
    internal static partial DeleteUserCommand ToCommand(Guid id);
}
