using PANiXiDA.TacticalHeroes.Modules.Identity.Application.Roles.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Identity.Presentation.Features.Roles.Delete;

[Mapper]
internal static partial class DeleteRoleMapper
{
    internal static partial DeleteRoleCommand ToCommand(Guid id);
}
