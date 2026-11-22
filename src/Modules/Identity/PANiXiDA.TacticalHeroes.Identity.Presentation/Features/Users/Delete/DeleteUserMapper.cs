using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Users.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Users.Delete;

[Mapper]
internal static partial class DeleteUserMapper
{
    internal static partial DeleteUserCommand ToCommand(Guid id);
}
