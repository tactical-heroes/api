using PANiXiDA.TacticalHeroes.Identity.Application.Users.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Users.Write.Delete;

[Mapper]
internal static partial class DeleteUserMapper
{
    internal static partial DeleteUserCommand ToCommand(Guid id);
}
