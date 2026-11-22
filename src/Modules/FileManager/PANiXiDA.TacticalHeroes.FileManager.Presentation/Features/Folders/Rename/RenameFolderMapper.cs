using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Rename;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Folders.Rename;

[Mapper]
internal static partial class RenameFolderMapper
{
    internal static partial RenameFolderCommand ToCommand(RenameFolderRequest request, Guid id);
}
