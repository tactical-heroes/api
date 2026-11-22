using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Delete;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.Delete;

[Mapper]
internal static partial class DeleteFolderMapper
{
    internal static partial DeleteFolderCommand ToCommand(Guid id);
}
