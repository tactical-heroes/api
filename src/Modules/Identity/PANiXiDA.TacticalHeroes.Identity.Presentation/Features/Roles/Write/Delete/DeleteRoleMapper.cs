using PANiXiDA.TacticalHeroes.Identity.Application.Roles.Write.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Roles.Write.Delete;

[Mapper]
internal static partial class DeleteRoleMapper
{
    internal static partial DeleteRoleCommand ToCommand(Guid id);
}
